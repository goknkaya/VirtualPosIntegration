using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using VirtualPos.Application.Orders;
using VirtualPos.Domain.Entities;

namespace VirtualPos.Infrastructure.Persistence;

public class OrderRepository(VirtualPosDbContext db) : IOrderRepository
{
    public Task AddAsync(Order order, CancellationToken ct = default)
    {
        db.Orders.Add(order);
        return Task.CompletedTask;
    }

    public Task<Order?> GetByOrderNoAsync(Guid orderNo, CancellationToken ct = default) =>
        db.Orders.FirstOrDefaultAsync(o => o.OrderNo == orderNo, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}
