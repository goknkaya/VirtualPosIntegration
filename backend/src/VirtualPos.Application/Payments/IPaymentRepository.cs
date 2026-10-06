using System;
using System.Collections.Generic;
using System.Text;
using VirtualPos.Domain.Entities;

namespace VirtualPos.Application.Payments;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment, CancellationToken ct = default);
    Task<Payment?> GetByOrderNoAsync(Guid orderNo, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
