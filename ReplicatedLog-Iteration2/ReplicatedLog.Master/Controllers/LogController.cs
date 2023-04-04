using Microsoft.AspNetCore.Mvc;
using ReplicatedLog.Common.Exceptions;
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
    public async Task<IActionResult> AddMessage([FromBody] string message)
    {
        try
        {
            _service.AppendMessageToLog(message);
        }
        catch(ConnectionFailureException ex)
        {
            return StatusCode(StatusCodes.Status408RequestTimeout, "Replication is currently blocked due to a connection failure with the Secondary server. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error during append to log message: {message}, ex:{ex.Message}", message, ex.Message);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, ex.Message);
        }

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetMessages()
    {
        return Ok(_service.GetAllMessages().Select(m => m.Msg));
    }

}