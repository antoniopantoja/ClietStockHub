using ClietStockHub.Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace ClietStockHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(ApiEnvelope.Success(new
        {
            status = "healthy"
        }));
    }

    [HttpGet("/health")]
    public IActionResult RootHealth()
    {
        return Ok(ApiEnvelope.Success(new
        {
            status = "healthy"
        }));
    }
}