using ClietStockHub.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClietStockHub.Api.Controllers;

[ApiController]
[Route("api/orders-query")]
public class OrdersQueryController : ControllerBase
{
    private readonly OrderQueries _queries;
    public OrdersQueryController(OrderQueries queries)
    {
        _queries = queries;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var result = await _queries.ListOrdersAsync();
        return Ok(result);
    }
}
