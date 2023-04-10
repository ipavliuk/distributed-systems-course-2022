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
    public async Task<IActionResult> AddMessage([FromBody] string message, [FromQuery] int writeConcern = 3)
    {
        if (writeConcern < 1 || writeConcern > 3)
        {
            return BadRequest("Write Concern should be in range [1,3]");
        }

        try
        {
            await _service.AppendMessageToLog(message, writeConcern);
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