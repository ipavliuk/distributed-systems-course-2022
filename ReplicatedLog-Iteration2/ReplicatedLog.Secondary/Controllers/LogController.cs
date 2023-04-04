using Common.Model;
using Common.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ReplicatedLog.Secondary.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LogController : ControllerBase
{
    private readonly IRepository _repository;
    private readonly ILogger _logger;
    public LogController(IRepository repository, ILogger<LogController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> AppendMessage(Message message)
    {
        _logger.LogInformation("Secondary append message {message.Id}", message.SequenceId);
        _repository.Add(message);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetMessages()
    {
        int expected = 1; // [TODO] initialize the expected value to the first item from master!!!!!!
        return Ok(_repository.GetAll().TakeWhile(m =>
        {
            bool result = m.SequenceId == expected; // check if the current item is equal to the expected value
            expected++; // increment the expected value for the next iteration
            return result;
        }).Select(m => m.Msg));

        //return Ok(_repository.GetAll().Select(m => m.Msg));
    }
}