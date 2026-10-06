using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using VirtualPos.Application.Bins;
using VirtualPos.Application.Installments;
using VirtualPos.Domain.Entities;
using VirtualPos.Domain.Enums;

namespace VirtualPos.Application.Payments;

public class PaymentService(
    IPaymentRepository repository,
    IBinProvider binProvider,
    InstallmentService installmentService,
    IPaymentProvider provider,
    PaymentSettings settings)
{
    // Demo: gerçekte tutar sipariş tablosundan okunur, asla istemciden alınmaz
    public const decimal OrderAmount = 1250m;

    // 1) Ödemeyi başlat
    public async Task<InitializePaymentResult> InitializeAsync(InitializePaymentRequest request, CancellationToken ct = default)
    {
        var cardNumber = new string(request.CardNumber.Where(char.IsAsciiDigit).ToArray());

        var validationError = Validate(request, cardNumber);
        if (validationError is not null)
            return Fail(validationError);

        var bin = cardNumber[..8];
        var binInfo = await binProvider.GetBinInfoAsync(bin, ct);
        if (binInfo is null)
            return Fail("Kartın bankası bulunamadı.");

        // Taksitli toplam tutar sunucuda hesaplanır
        var options = await installmentService.GetOptionsAsync(bin, OrderAmount, ct);
        var option = options?.FirstOrDefault(o => o.Count == request.Installment);
        if (option is null)
            return Fail("Seçilen taksit bu kart için geçerli değil.");

        var payment = new Payment
        {
            PaymentNo = Guid.NewGuid(),
            Amount = option.TotalAmount,
            Currency = "TRY",
            Installment = (byte)option.Count,
            CardHolderName = request.CardHolderName.Trim(),
            MaskedCardNumber = CardMasker.Mask(cardNumber),
            BinNumber = bin,
            BankEftCode = binInfo.BankEftCode,
            BankName = binInfo.BankName,
            Provider = provider.Name,
            Status = PaymentStatus.Pending3DS,
            CreatedAt = DateTime.UtcNow
        };
        await repository.AddAsync(payment, ct);

        var initResult = await provider.InitializeThreeDsAsync(new ThreeDsInitRequest(
            payment.PaymentNo,
            payment.Amount,
            payment.Currency,
            payment.Installment,
            payment.CardHolderName,
            cardNumber,
            request.ExpireMonth,
            request.ExpireYear,
            request.Cvc,
            CallbackUrl: $"{settings.ApiBaseUrl}/api/payments/3ds/callback"), ct);

        AddLog(payment, "Initialize",
            new { payment.MaskedCardNumber, payment.Amount, payment.Installment },
            new { initResult.IsSuccess, initResult.ProviderPaymentId, initResult.ErrorCode, initResult.ErrorMessage },
            initResult.IsSuccess);

        if (initResult.IsSuccess)
            payment.ProviderPaymentId = initResult.ProviderPaymentId;
        else
            MarkFailed(payment, initResult.ErrorCode, initResult.ErrorMessage);

        await repository.SaveChangesAsync(ct);

        return initResult.IsSuccess
            ? new InitializePaymentResult(true, payment.PaymentNo, initResult.RedirectUrl, initResult.HtmlContent, null)
            : Fail(initResult.ErrorMessage ?? "Ödeme başlatılamadı.");
    }

    // 2) Bankadan dönüşü işle
    public async Task<Guid?> HandleCallbackAsync(ThreeDsCallback callback, CancellationToken ct = default)
    {
        if (!Guid.TryParse(callback.ConversationId, out var orderNo))
            return null;

        var payment = await repository.GetByOrderNoAsync(orderNo, ct);
        if (payment is null)
            return null;

        // Idempotency: zaten sonuçlanmış ödeme ikinci kez işlenmez
        if (payment.Status != PaymentStatus.Pending3DS)
            return payment.PaymentNo;

        payment.MdStatus = callback.MdStatus;

        var isVerified = callback.Status == "success"
                         && callback.MdStatus == "1"
                         && callback.PaymentId == payment.ProviderPaymentId;

        AddLog(payment, "Callback", callback, null, isVerified);

        if (!isVerified)
        {
            MarkFailed(payment, "3DS_FAILED", "3D Secure doğrulaması başarısız oldu.");
        }
        else
        {
            var complete = await provider.CompleteThreeDsAsync(payment.ProviderPaymentId!, payment.PaymentNo, ct);
            AddLog(payment, "Complete", new { payment.ProviderPaymentId }, complete, complete.IsSuccess);

            if (complete.IsSuccess)
            {
                payment.Status = PaymentStatus.Succeeded;
                payment.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                MarkFailed(payment, complete.ErrorCode, complete.ErrorMessage);
            }
        }

        await repository.SaveChangesAsync(ct);
        return payment.PaymentNo;
    }

    // 3) Sonuç sayfası için ödemeyi getir
    public async Task<PaymentResultDto?> GetAsync(Guid orderNo, CancellationToken ct = default)
    {
        var p = await repository.GetByOrderNoAsync(orderNo, ct);
        return p is null
            ? null
            : new PaymentResultDto(p.PaymentNo, p.Status.ToString(), p.Amount, p.Installment,
                p.MaskedCardNumber, p.BankName, p.ErrorMessage, p.CreatedAt);
    }

    private static string? Validate(InitializePaymentRequest r, string cardNumber)
    {
        if (r.CardHolderName.Trim().Length < 3) return "Kart üzerindeki isim geçersiz.";
        if (cardNumber.Length is < 15 or > 16 || !CardValidator.IsValidLuhn(cardNumber)) return "Kart numarası geçersiz.";
        if (!CardValidator.IsValidExpiry(r.ExpireMonth, r.ExpireYear)) return "Son kullanma tarihi geçersiz.";
        if (r.Cvc.Length is < 3 or > 4 || !r.Cvc.All(char.IsAsciiDigit)) return "CVV geçersiz.";
        return null;
    }

    private static InitializePaymentResult Fail(string message) =>
        new(false, null, null, null, message);

    private static void MarkFailed(Payment p, string? code, string? message)
    {
        p.Status = PaymentStatus.Failed;
        p.ErrorCode = code;
        p.ErrorMessage = message;
        p.UpdatedAt = DateTime.UtcNow;
    }

    // Log'a asla tam kart numarası veya CVV yazılmaz
    private static void AddLog(Payment p, string step, object request, object? response, bool isSuccess) =>
        p.Logs.Add(new PaymentLog
        {
            Step = step,
            RequestJson = JsonSerializer.Serialize(request),
            ResponseJson = response is null ? null : JsonSerializer.Serialize(response),
            IsSuccess = isSuccess,
            CreatedAt = DateTime.UtcNow
        });
}
