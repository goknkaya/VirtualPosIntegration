using VirtualPos.Domain.Entities;

namespace VirtualPos.Application.Payments;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment, CancellationToken ct = default);
    Task<Payment?> GetByPaymentNoAsync(Guid paymentNo, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}