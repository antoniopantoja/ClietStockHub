using ClietStockHub.Domain.Entities;
using ClietStockHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClietStockHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<CustomersController> _logger;
    public CustomersController(AppDbContext db, ILogger<CustomersController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var customers = await _db.Customers.AsNoTracking().ToListAsync();
        _logger.LogInformation("Listando todos os clientes. Total: {Count}", customers.Count);
        return Ok(customers);
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
