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
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        var all = await _queries.ListOrdersAsync();
        var total = all.Count();
        var items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Ok(new {
            page,
            pageSize,
            total,
            items
        });
    }

    [HttpGet("by-customer/{customerId}")]
    public async Task<IActionResult> ListByCustomer(Guid customerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        var all = await _queries.ListOrdersByCustomerAsync(customerId);
        var total = all.Count();
        var items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Ok(new {
            page,
            pageSize,
            total,
            items
        });
    }
}
