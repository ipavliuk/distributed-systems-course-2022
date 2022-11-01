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
        _logger.LogInformation("Secondary append message {message.Id}", message.Id);
        _repository.Add(message);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetMessages()
    {
        return Ok(_repository.GetAll().Select(m => m.Msg));
    }
}