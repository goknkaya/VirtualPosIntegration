using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using VirtualPos.Application.Notifications;
using VirtualPos.Application.Payments;
using VirtualPos.Infrastructure.MockBank;

namespace VirtualPos.Infrastructure.Payments;

public class MockPaymentProvider(
    MockBankSessionStore store,
    ISmsSender smsSender,
    MockBankSettings settings) : IPaymentProvider
{
    // Demo: kart sahibinin bankada kayıtlı telefonu
    private const string RegisteredPhone = "0555 *** ** 34";

    public string Name => "MockBank";

    public async Task<ThreeDsInitResult> InitializeThreeDsAsync(ThreeDsInitRequest request, CancellationToken ct = default)
    {
        var token = Guid.NewGuid().ToString("N");
        var otpCode = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

        store.Add(new MockBankSession
        {
            Token = token,
            OrderNo = request.OrderNo,
            Amount = request.Amount,
            MaskedCardNumber = CardMasker.Mask(request.CardNumber),
            CallbackUrl = request.CallbackUrl,
            OtpCode = otpCode,
            ExpiresAt = DateTime.UtcNow.AddMinutes(3)
        });

        await smsSender.SendAsync(
            RegisteredPhone,
            $"{request.Amount:N2} TL tutarındaki ödemeniz için doğrulama kodunuz: {otpCode}",
            ct);

        return new ThreeDsInitResult(
            IsSuccess: true,
            ProviderPaymentId: token,
            RedirectUrl: $"{settings.ApiBaseUrl}/mock-bank/3ds/{token}",
            HtmlContent: null,
            ErrorCode: null,
            ErrorMessage: null);
    }

    public Task<ThreeDsCompleteResult> CompleteThreeDsAsync(string providerPaymentId, Guid orderNo, CancellationToken ct = default)
    {
        var session = store.Get(providerPaymentId);

        if (session is null || session.OrderNo != orderNo)
            return Task.FromResult(new ThreeDsCompleteResult(false, "PAYMENT_NOT_FOUND", "Ödeme bulunamadı."));

        // Banka, callback'te ne yazdığına değil kendi kaydına bakar
        if (!session.IsVerified)
            return Task.FromResult(new ThreeDsCompleteResult(false, "3DS_NOT_VERIFIED", "3D Secure doğrulaması yapılmadı."));

        if (session.IsCompleted)
            return Task.FromResult(new ThreeDsCompleteResult(false, "ALREADY_COMPLETED", "Bu ödeme zaten tamamlandı."));

        session.IsCompleted = true;
        return Task.FromResult(new ThreeDsCompleteResult(true, null, null));
    }
}