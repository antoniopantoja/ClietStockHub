using ClietStockHub.Application.Dtos;
using ClietStockHub.Domain.Entities;
using ClietStockHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClietStockHub.Application.Services;

public class OrderService
{
    private readonly AppDbContext _db;
    private readonly ILogger<OrderService> _logger;

    public OrderService(AppDbContext db, ILogger<OrderService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<(bool Success, string? Error, Order? Order)> CreateOrderAsync(CreateOrderRequest request, string idempotencyKey)
    {
        // Idempotência: verifica se já existe pedido com esse idempotencyKey
        _logger.LogInformation("Verificando idempotência para chave: {Key}", idempotencyKey);
        var existing = await _db.Orders.FirstOrDefaultAsync(o => o.Status == idempotencyKey);
        if (existing != null)
        {
            _logger.LogWarning("Pedido já existe para chave de idempotência: {Key}", idempotencyKey);
            return (true, null, existing);
        }

        var isInMemory = _db.Database.ProviderName?.Contains("InMemory") == true;
        var tx = isInMemory ? null : await _db.Database.BeginTransactionAsync();
        try
        {
            var customer = await _db.Customers.FindAsync(request.CustomerId);

            if (customer == null)
            {
                _logger.LogWarning("Cliente não encontrado: {CustomerId}", request.CustomerId);
                return (false, "Cliente não encontrado", null);
            }

            var productIds = request.Items.Select(i => i.ProductId).ToList();
            var products = await _db.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

            if (products.Count != productIds.Count)
            {
                _logger.LogWarning("Um ou mais produtos não encontrados. IDs requisitados: {Ids}", string.Join(",", productIds));
                return (false, "Um ou mais produtos não encontrados", null);
            }

            // Validação de estoque

            foreach (var item in request.Items)
            {
                var prod = products.First(p => p.Id == item.ProductId);
                if (prod.StockQty < item.Quantity)
                {
                    _logger.LogWarning("Estoque insuficiente para produto {ProductId}", prod.Id);
                    return (false, $"Estoque insuficiente para o produto {prod.Name}", null);
                }
            }

            // Atualiza estoque
            foreach (var item in request.Items)
            {
                var prod = products.First(p => p.Id == item.ProductId);
                prod.StockQty -= item.Quantity;
            }

            var order = new Order
            {
                CustomerId = request.CustomerId,
                CreatedAt = DateTime.UtcNow,
                Status = idempotencyKey, // Para simplificação, armazena o idempotencyKey no status
                Items = new List<OrderItem>()
            };
            decimal total = 0;
            foreach (var item in request.Items)
            {
                var prod = products.First(p => p.Id == item.ProductId);
                var lineTotal = prod.Price * item.Quantity;
                order.Items.Add(new OrderItem
                {
                    ProductId = prod.Id,
                    UnitPrice = prod.Price,
                    Quantity = item.Quantity,
                    LineTotal = lineTotal
                });
                total += lineTotal;
            }
            order.TotalAmount = total;
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
            if (tx != null)
                await tx.CommitAsync();
            _logger.LogInformation("Pedido criado com sucesso: {OrderId}", order.Id);
            return (true, null, order);
        }
        catch (Exception ex)
        {
            if (tx != null)
                await tx.RollbackAsync();
            _logger.LogError(ex, "Erro ao criar pedido");
            return (false, "Erro ao criar pedido", null);
        }
    }
}
