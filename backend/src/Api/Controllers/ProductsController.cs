using ClietStockHub.Domain.Entities;
using ClietStockHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClietStockHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(Guid id, [FromBody] Dictionary<string, object> patch)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();
        bool changed = false;
        foreach (var kv in patch)
        {
            switch (kv.Key.ToLower())
            {
                case "name": product.Name = kv.Value?.ToString() ?? product.Name; changed = true; break;
                case "sku": product.Sku = kv.Value?.ToString() ?? product.Sku; changed = true; break;
                case "price": if (decimal.TryParse(kv.Value?.ToString(), out var price)) { product.Price = price; changed = true; } break;
                case "stockqty": if (int.TryParse(kv.Value?.ToString(), out var qty)) { product.StockQty = qty; changed = true; } break;
                case "isactive": if (bool.TryParse(kv.Value?.ToString(), out var active)) { product.IsActive = active; changed = true; } break;
            }
        }
        if (!changed) return BadRequest(new { error = "Nenhum campo válido para atualizar." });
        await _db.SaveChangesAsync();
        _logger.LogInformation("Produto parcialmente atualizado: {Id}", product.Id);
        return Ok(product);
    }
    private readonly AppDbContext _db;
    private readonly ILogger<ProductsController> _logger;
    public ProductsController(AppDbContext db, ILogger<ProductsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        var query = _db.Products.AsNoTracking();
        var total = await query.CountAsync();
        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        _logger.LogInformation("Listando produtos paginados. Página: {Page}, Tamanho: {PageSize}, Total: {Total}", page, pageSize, total);
        return Ok(new {
            page,
            pageSize,
            total,
            items = products
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Product product)
    {
        product.CreatedAt = DateTime.UtcNow;
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Produto criado: {Id} - {Name}", product.Id, product.Name);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, Product updated)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();
        product.Name = updated.Name;
        product.Sku = updated.Sku;
        product.Price = updated.Price;
        product.StockQty = updated.StockQty;
        product.IsActive = updated.IsActive;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Produto atualizado: {Id}", product.Id);
        return Ok(product);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();
        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Produto removido: {Id}", id);
        return NoContent();
    }
}
