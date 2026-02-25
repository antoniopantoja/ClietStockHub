using ClietStockHub.Domain.Entities;
using ClietStockHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClietStockHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(Guid id, [FromBody] Dictionary<string, object> patch)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer == null) return NotFound();
        bool changed = false;
        foreach (var kv in patch)
        {
            switch (kv.Key.ToLower())
            {
                case "name": customer.Name = kv.Value?.ToString() ?? customer.Name; changed = true; break;
                case "email": customer.Email = kv.Value?.ToString() ?? customer.Email; changed = true; break;
                case "document": customer.Document = kv.Value?.ToString() ?? customer.Document; changed = true; break;
            }
        }
        if (!changed) return BadRequest(new { error = "Nenhum campo válido para atualizar." });
        await _db.SaveChangesAsync();
        _logger.LogInformation("Cliente parcialmente atualizado: {Id}", customer.Id);
        return Ok(customer);
    }

    private readonly AppDbContext _db;
    private readonly ILogger<CustomersController> _logger;
    public CustomersController(AppDbContext db, ILogger<CustomersController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        var query = _db.Customers.AsNoTracking();
        var total = await query.CountAsync();
        var customers = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        _logger.LogInformation("Listando clientes paginados. Página: {Page}, Tamanho: {PageSize}, Total: {Total}", page, pageSize, total);
        return Ok(new {
            page,
            pageSize,
            total,
            items = customers
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer == null) return NotFound();
        return Ok(customer);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Customer customer)
    {
        customer.CreatedAt = DateTime.UtcNow;
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Cliente criado: {Id} - {Name}", customer.Id, customer.Name);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, Customer updated)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer == null) return NotFound();
        customer.Name = updated.Name;
        customer.Email = updated.Email;
        customer.Document = updated.Document;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Cliente atualizado: {Id}", customer.Id);
        return Ok(customer);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer == null) return NotFound();
        _db.Customers.Remove(customer);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Cliente removido: {Id}", id);
        return NoContent();
    }
}
