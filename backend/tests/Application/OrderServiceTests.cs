using System.Collections.Generic;
using System.Threading.Tasks;
using ClietStockHub.Application.Dtos;
using ClietStockHub.Application.Services;
using ClietStockHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ClietStockHub.Infrastructure.Persistence;

namespace ClietStockHub.Tests.Application;

public class OrderServiceTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "OrderServiceTests")
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task NaoPermitePedidoComEstoqueInsuficiente()
    {
        var db = GetDbContext();
        var customerId = System.Guid.NewGuid();
        var productId = System.Guid.NewGuid();
        db.Customers.Add(new Customer { Id = customerId, Name = "Cliente", Email = "a@a.com", Document = "123", CreatedAt = System.DateTime.UtcNow });
        db.Products.Add(new Product { Id = productId, Name = "Produto", Sku = "SKU1", Price = 10, StockQty = 1, IsActive = true, CreatedAt = System.DateTime.UtcNow });
        db.SaveChanges();
        var logger = Mock.Of<ILogger<OrderService>>();
        var service = new OrderService(db, logger);
        var req = new CreateOrderRequest
        {
            CustomerId = customerId,
            Items = new List<CreateOrderItemRequest> { new() { ProductId = productId, Quantity = 2 } }
        };
        var (success, error, order) = await service.CreateOrderAsync(req, "key1");
        Assert.False(success);
        Assert.Contains("Estoque insuficiente", error);
    }

    [Fact]
    public async Task CriaPedidoComSucessoEDescontaEstoque()
    {
        var db = GetDbContext();
        var customerId = System.Guid.NewGuid();
        var productId = System.Guid.NewGuid();
        db.Customers.Add(new Customer { Id = customerId, Name = "Cliente2", Email = "b@b.com", Document = "456", CreatedAt = System.DateTime.UtcNow });
        db.Products.Add(new Product { Id = productId, Name = "Produto2", Sku = "SKU2", Price = 20, StockQty = 5, IsActive = true, CreatedAt = System.DateTime.UtcNow });
        db.SaveChanges();
        var logger = Mock.Of<ILogger<OrderService>>();
        var service = new OrderService(db, logger);
        var req = new CreateOrderRequest
        {
            CustomerId = customerId,
            Items = new List<CreateOrderItemRequest> { new() { ProductId = productId, Quantity = 3 } }
        };
        var (success, error, order) = await service.CreateOrderAsync(req, "key2");
        Assert.True(success);
        Assert.NotNull(order);
        Assert.Equal(customerId, order.CustomerId);
        Assert.Equal(3, order.Items[0].Quantity);
        Assert.Equal(2, db.Products.Find(productId)!.StockQty);
    }

    [Fact]
    public async Task GaranteIdempotencia()
    {
        var db = GetDbContext();
        var customerId = System.Guid.NewGuid();
        var productId = System.Guid.NewGuid();
        db.Customers.Add(new Customer { Id = customerId, Name = "Cliente3", Email = "c@c.com", Document = "789", CreatedAt = System.DateTime.UtcNow });
        db.Products.Add(new Product { Id = productId, Name = "Produto3", Sku = "SKU3", Price = 30, StockQty = 10, IsActive = true, CreatedAt = System.DateTime.UtcNow });
        db.SaveChanges();
        var logger = Mock.Of<ILogger<OrderService>>();
        var service = new OrderService(db, logger);
        var req = new CreateOrderRequest
        {
            CustomerId = customerId,
            Items = new List<CreateOrderItemRequest> { new() { ProductId = productId, Quantity = 1 } }
        };
        var (success1, _, order1) = await service.CreateOrderAsync(req, "key3");
        var (success2, _, order2) = await service.CreateOrderAsync(req, "key3");
        Assert.True(success1);
        Assert.True(success2);
        Assert.Equal(order1!.Id, order2!.Id);
    }
}
