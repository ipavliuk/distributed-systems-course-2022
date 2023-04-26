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
    private readonly IConfiguration _configuration;

    public LogController(IRepository repository, ILogger<LogController> logger, IConfiguration configuration)
    {
        _repository = repository;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<IActionResult> AppendMessage(Message message)
    {
        //for testing purposes
        int? appendMsgTimeout = _configuration.GetSection("AppendMessageResponseTimeOut")?.Get<int>();
        if (appendMsgTimeout != null && appendMsgTimeout > 0)
        {
            Thread.Sleep((int)appendMsgTimeout);
        }
        
        _repository.Add(message);
        _logger.LogInformation("Secondary appended message {message.Id}", message.SequenceId);

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

    }
}