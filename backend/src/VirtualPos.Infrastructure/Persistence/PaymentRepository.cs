using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using VirtualPos.Application.Payments;
using VirtualPos.Domain.Entities;

namespace VirtualPos.Infrastructure.Persistence;

public class PaymentRepository(VirtualPosDbContext db) : IPaymentRepository
{
    public Task AddAsync(Payment payment, CancellationToken ct = default)
    {
        db.Payments.Add(payment);
        return Task.CompletedTask;
    }

    public Task<Payment?> GetByOrderNoAsync(Guid orderNo, CancellationToken ct = default) =>
        db.Payments.FirstOrDefaultAsync(p => p.OrderNo == orderNo, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}