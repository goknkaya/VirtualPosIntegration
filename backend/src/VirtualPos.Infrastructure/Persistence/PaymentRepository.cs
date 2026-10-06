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

    public Task<Payment?> GetByPaymentNoAsync(Guid paymentNo, CancellationToken ct = default) =>
        db.Payments
          .Include(p => p.Order)
          .FirstOrDefaultAsync(p => p.PaymentNo == paymentNo, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}