using System.Text.Json;
using VirtualPos.Application.Bins;
using VirtualPos.Application.Installments;
using VirtualPos.Application.Orders;
using VirtualPos.Domain.Entities;
using VirtualPos.Domain.Enums;

namespace VirtualPos.Application.Payments;

public class PaymentService(
    IPaymentRepository payments,
    IOrderRepository orders,
    IBinProvider binProvider,
    InstallmentService installmentService,
    IPaymentProvider provider,
    PaymentSettings settings)
{
    // 1) Ödemeyi başlat
    public async Task<InitializePaymentResult> InitializeAsync(InitializePaymentRequest request, CancellationToken ct = default)
    {
        var cardNumber = new string(request.CardNumber.Where(char.IsAsciiDigit).ToArray());

        var validationError = Validate(request, cardNumber);
        if (validationError is not null)
            return Fail(validationError);

        // Tutar her zaman siparişten gelir
        var order = await orders.GetByOrderNoAsync(request.OrderNo, ct);
        if (order is null)
            return Fail("Sipariş bulunamadı.");
        if (order.Status == OrderStatus.Paid)
            return Fail("Bu sipariş zaten ödenmiş.");

        var bin = cardNumber[..8];
        var binInfo = await binProvider.GetBinInfoAsync(bin, ct);
        if (binInfo is null)
            return Fail("Kartın bankası bulunamadı.");

        var options = await installmentService.GetOptionsAsync(bin, order.Amount, ct);
        var option = options?.FirstOrDefault(o => o.Count == request.Installment);
        if (option is null)
            return Fail("Seçilen taksit bu kart için geçerli değil.");

        var payment = new Payment
        {
            PaymentNo = Guid.NewGuid(),
            Order = order,
            Amount = option.TotalAmount,
            Currency = order.Currency,
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
        await payments.AddAsync(payment, ct);

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
            order.BuyerPhone,
            CallbackUrl: $"{settings.ApiBaseUrl}/api/payments/3ds/callback"), ct);

        AddLog(payment, "Initialize",
            new { order.OrderNo, payment.MaskedCardNumber, payment.Amount, payment.Installment },
            new { initResult.IsSuccess, initResult.ProviderPaymentId, initResult.ErrorCode, initResult.ErrorMessage },
            initResult.IsSuccess);

        if (initResult.IsSuccess)
            payment.ProviderPaymentId = initResult.ProviderPaymentId;
        else
            MarkFailed(payment, initResult.ErrorCode, initResult.ErrorMessage);

        await payments.SaveChangesAsync(ct);

        return initResult.IsSuccess
            ? new InitializePaymentResult(true, payment.PaymentNo, initResult.RedirectUrl, initResult.HtmlContent, null)
            : Fail(initResult.ErrorMessage ?? "Ödeme başlatılamadı.");
    }

    // 2) Bankadan dönüşü işle
    public async Task<Guid?> HandleCallbackAsync(ThreeDsCallback callback, CancellationToken ct = default)
    {
        if (!Guid.TryParse(callback.ConversationId, out var paymentNo))
            return null;

        var payment = await payments.GetByPaymentNoAsync(paymentNo, ct);
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
        else if (payment.Order.Status == OrderStatus.Paid)
        {
            // Aynı sipariş başka bir ödemeyle zaten ödendiyse ikinci kez para çekme
            MarkFailed(payment, "ORDER_ALREADY_PAID", "Sipariş başka bir ödemeyle zaten ödendi.");
        }
        else
        {
            var complete = await provider.CompleteThreeDsAsync(payment.ProviderPaymentId!, payment.PaymentNo, ct);
            AddLog(payment, "Complete", new { payment.ProviderPaymentId }, complete, complete.IsSuccess);

            if (complete.IsSuccess)
            {
                payment.Status = PaymentStatus.Succeeded;
                payment.UpdatedAt = DateTime.UtcNow;
                payment.Order.Status = OrderStatus.Paid;
                payment.Order.PaidAt = DateTime.UtcNow;
            }
            else
            {
                MarkFailed(payment, complete.ErrorCode, complete.ErrorMessage);
            }
        }

        await payments.SaveChangesAsync(ct);
        return payment.PaymentNo;
    }

    // 3) Sonuç sayfası için ödemeyi getir
    public async Task<PaymentResultDto?> GetAsync(Guid paymentNo, CancellationToken ct = default)
    {
        var p = await payments.GetByPaymentNoAsync(paymentNo, ct);
        return p is null
            ? null
            : new PaymentResultDto(p.PaymentNo, p.Order.OrderNo, p.Status.ToString(), p.Amount, p.Installment,
                p.MaskedCardNumber, p.BankName, p.ErrorMessage,
                DateTime.SpecifyKind(p.CreatedAt, DateTimeKind.Utc));
    }

    // 4) Süresinde tamamlanmayan ödemeleri kapat (arka plan işi çağırır)
    public static readonly TimeSpan PendingTimeout = TimeSpan.FromMinutes(5);

    public async Task<int> ExpirePendingPaymentsAsync(CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow - PendingTimeout;
        var expired = await payments.GetPendingOlderThanAsync(cutoff, ct);

        foreach (var payment in expired)
        {
            MarkFailed(payment, "TIMEOUT", "3D Secure doğrulaması süresinde tamamlanmadı.");
            AddLog(payment, "Timeout", new { CutoffUtc = cutoff }, null, false);
        }

        if (expired.Count > 0)
            await payments.SaveChangesAsync(ct);

        return expired.Count;
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
        p.ErrorCode = code is { Length: > 50 } ? code[..50] : code;
        p.ErrorMessage = message is { Length: > 500 } ? message[..500] : message;
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