using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Microsoft.Extensions.Configuration;

namespace ClietStockHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DbTestController : ControllerBase
{
    private readonly IConfiguration _config;
    public DbTestController(IConfiguration config)
    {
        _config = config;
    }

    /// <summary>
    /// Testa conexão com o banco PostgreSQL configurado.
    /// </summary>
    [HttpGet("ping")]
    public async Task<IActionResult> Ping()
    {
        var connStr = _config.GetConnectionString("DefaultConnection");
        try
        {
            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand("SELECT 1", conn);
            var result = await cmd.ExecuteScalarAsync();
            return Ok(new { status = "ok", db = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = "fail", error = ex.Message });
        }
    }
}
