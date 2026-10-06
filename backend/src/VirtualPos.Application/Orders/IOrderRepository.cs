using System;
using System.Collections.Generic;
using System.Text;
using VirtualPos.Domain.Entities;

namespace VirtualPos.Application.Orders;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken ct = default);
    Task<Order?> GetByOrderNoAsync(Guid orderNo, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
