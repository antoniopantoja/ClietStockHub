using ClietStockHub.Application.Dtos;
using ClietStockHub.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClietStockHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;
    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        if (!Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey) || string.IsNullOrWhiteSpace(idempotencyKey))
            return BadRequest(new { error = "Header Idempotency-Key obrigatório" });

        var (success, error, order) = await _orderService.CreateOrderAsync(request, idempotencyKey!);
        if (!success)
            return BadRequest(new { error });
        return Ok(order);
    }
}
