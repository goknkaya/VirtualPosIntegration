namespace VirtualPos.Application.Payments;

public interface IPaymentProvider
{
    string Name { get; }

    Task<ThreeDsInitResult> InitializeThreeDsAsync(ThreeDsInitRequest request, CancellationToken ct = default);

    Task<ThreeDsCompleteResult> CompleteThreeDsAsync(string providerPaymentId, Guid paymentNo, CancellationToken ct = default);
}