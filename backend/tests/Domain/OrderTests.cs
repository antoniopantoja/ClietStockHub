using System;
using System.Collections.Generic;
using ClietStockHub.Domain.Entities;
using Xunit;

namespace ClietStockHub.Tests.Domain;

public class OrderTests
{
    [Fact]
    public void CriaOrderComValoresPadrao()
    {
        var order = new Order();
        Assert.Equal(OrderStatus.CREATED, order.Status);
        Assert.NotNull(order.Items);
        Assert.Empty(order.Items);
        Assert.Equal(Guid.Empty, order.Id);
        Assert.Equal(Guid.Empty, order.CustomerId);
        Assert.Equal(0, order.TotalAmount);
        Assert.True(order.CreatedAt == default);
    }

    [Fact]
    public void PermiteAdicionarItensEAtualizarTotal()
    {
        var order = new Order
        {
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
        order.Items.Add(new OrderItem { ProductId = Guid.NewGuid(), UnitPrice = 10, Quantity = 2, LineTotal = 20 });
        order.Items.Add(new OrderItem { ProductId = Guid.NewGuid(), UnitPrice = 5, Quantity = 1, LineTotal = 5 });
        order.TotalAmount = 25;
        Assert.Equal(2, order.Items.Count);
        Assert.Equal(25, order.TotalAmount);
    }

    [Fact]
    public void PermiteAlterarStatus()
    {
        var order = new Order();
        order.Status = OrderStatus.PAID;
        Assert.Equal(OrderStatus.PAID, order.Status);
        order.Status = OrderStatus.CANCELLED;
        Assert.Equal(OrderStatus.CANCELLED, order.Status);
    }
}
