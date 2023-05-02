using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ReplicatedLog.Secondary.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HealthController : ControllerBase
{

    [HttpGet]
    public async Task<IActionResult> GetHealthStatus()
    {
        return Ok();
    }

}
