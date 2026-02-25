using ClietStockHub.Application.Dtos;
using ClietStockHub.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClietStockHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null) return NotFound();
        await _orderService.DeleteOrderAsync(order);
        return NoContent();
    }
    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(Guid id, [FromBody] Dictionary<string, object> patch)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null) return NotFound();
        bool changed = false;
        foreach (var kv in patch)
        {
            switch (kv.Key.ToLower())
            {
                case "status":
                    if (Enum.TryParse(typeof(ClietStockHub.Domain.Entities.OrderStatus), kv.Value?.ToString(), true, out var status))
                    {
                        order.Status = (ClietStockHub.Domain.Entities.OrderStatus)status;
                        changed = true;
                    }
                    break;
            }
        }
        if (!changed) return BadRequest(new { error = "Nenhum campo válido para atualizar." });
        await _orderService.SaveOrderAsync(order);
        return Ok(order);
    }

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
