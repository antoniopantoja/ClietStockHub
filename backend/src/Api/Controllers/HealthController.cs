using ClietStockHub.Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace ClietStockHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    /// <summary>
    /// Verifica saúde da API no prefixo padrão de controllers.
    /// </summary>
    /// <returns>Envelope com status da aplicação.</returns>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(ApiEnvelope.Success(new
        {
            status = "healthy"
        }));
    }

    /// <summary>
    /// Verifica saúde da API na raiz para integração com probes.
    /// </summary>
    /// <returns>Envelope com status da aplicação.</returns>
    [HttpGet("/health")]
    public IActionResult RootHealth()
    {
        return Ok(ApiEnvelope.Success(new
        {
            status = "healthy"
        }));
    }
}