using Microsoft.AspNetCore.Mvc;
using ReplicatedLog.Master.Services;

namespace ReplicatedLog.Master.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LogController : ControllerBase
{
    private readonly IReplicatedLogService _service;
    private readonly ILogger<LogController> _logger;

    public LogController(IReplicatedLogService service, ILogger<LogController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> AddMessage(string message)
    {
        try
        {
            _service.AppendMessageToLog(message);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error during append to log message: {message}", message);
            return BadRequest(ex.Message);
        }
        

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetMessages()
    {
        return Ok(_service.GetAllMessages().Select(m => m.Msg));
    }

}