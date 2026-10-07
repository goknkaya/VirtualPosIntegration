using System.Globalization;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using VirtualPos.Application.Payments;

namespace VirtualPos.Infrastructure.Payments;

public record IyzicoSettings(string ApiKey, string SecretKey, string BaseUrl);

public class IyzicoPaymentProvider(IyzicoSettings settings) : IPaymentProvider
{
    // Demo: iyzico alıcı ve adres bilgisini zorunlu istiyor; gerçek sistemde bunlar müşteri kaydından gelir
    private const string DemoEmail = "demo@example.com";
    private const string DemoIdentityNumber = "74300864791";
    private const string DemoIp = "85.34.78.112";
    private const string DemoAddress = "Demo Mah. Örnek Sok. No:1";
    private const string DemoCity = "Istanbul";
    private const string DemoCountry = "Turkey";
    private const string DemoZipCode = "34000";

    public string Name => "Iyzico";

    public async Task<ThreeDsInitResult> InitializeThreeDsAsync(ThreeDsInitRequest request, CancellationToken ct = default)
    {
        var price = ToIyzicoPrice(request.Amount);
        var (name, surname) = SplitName(request.CardHolderName);

        var iyzicoRequest = new CreatePaymentRequest
        {
            Locale = Locale.TR.ToString(),
            ConversationId = request.PaymentNo.ToString(),
            Price = price,
            PaidPrice = price,
            Currency = Currency.TRY.ToString(),
            Installment = request.Installment,
            BasketId = request.PaymentNo.ToString(),
            PaymentChannel = PaymentChannel.WEB.ToString(),
            PaymentGroup = PaymentGroup.PRODUCT.ToString(),
            CallbackUrl = request.CallbackUrl,
            PaymentCard = new PaymentCard
            {
                CardHolderName = request.CardHolderName,
                CardNumber = request.CardNumber,
                ExpireMonth = request.ExpireMonth,
                ExpireYear = "20" + request.ExpireYear,
                Cvc = request.Cvc,
                RegisterCard = 0
            },
            Buyer = new Buyer
            {
                Id = request.PaymentNo.ToString(),
                Name = name,
                Surname = surname,
                GsmNumber = ToGsmNumber(request.BuyerPhone),
                Email = DemoEmail,
                IdentityNumber = DemoIdentityNumber,
                RegistrationAddress = DemoAddress,
                Ip = DemoIp,
                City = DemoCity,
                Country = DemoCountry,
                ZipCode = DemoZipCode
            },
            ShippingAddress = DemoAddressFor(request.CardHolderName),
            BillingAddress = DemoAddressFor(request.CardHolderName),
            BasketItems =
            [
                new BasketItem
                {
                    Id = "ORDER",
                    Name = "Sipariş ödemesi",
                    Category1 = "Genel",
                    ItemType = BasketItemType.VIRTUAL.ToString(),
                    Price = price
                }
            ]
        };

        try
        {
            var response = await ThreedsInitialize.Create(iyzicoRequest, CreateOptions());

            if (!IsSuccess(response.Status))
                return new ThreeDsInitResult(false, null, null, null, response.ErrorCode, response.ErrorMessage);

            // iyzico link değil, bankanın 3DS sayfasının HTML'ini döner (kütüphane base64'ü çözmüş olarak verir)
            return new ThreeDsInitResult(true, response.PaymentId, null, response.HtmlContent, null, null);
        }
        catch (Exception ex)
        {
            return new ThreeDsInitResult(false, null, null, null, "IYZICO_UNREACHABLE", $"iyzico'ya ulaşılamadı: {ex.Message}");
        }
    }

    public async Task<ThreeDsCompleteResult> CompleteThreeDsAsync(string providerPaymentId, Guid paymentNo, CancellationToken ct = default)
    {
        var iyzicoRequest = new CreateThreedsPaymentRequest
        {
            Locale = Locale.TR.ToString(),
            ConversationId = paymentNo.ToString(),
            PaymentId = providerPaymentId
        };

        try
        {
            // Para burada çekilir: iyzico 3DS doğrulamasını kendi kaydından kontrol eder
            var response = await ThreedsPayment.Create(iyzicoRequest, CreateOptions());

            return IsSuccess(response.Status)
                ? new ThreeDsCompleteResult(true, null, null)
                : new ThreeDsCompleteResult(false, response.ErrorCode, response.ErrorMessage);
        }
        catch (Exception ex)
        {
            return new ThreeDsCompleteResult(false, "IYZICO_UNREACHABLE", $"iyzico'ya ulaşılamadı: {ex.Message}");
        }
    }

    private Options CreateOptions() => new()
    {
        ApiKey = settings.ApiKey,
        SecretKey = settings.SecretKey,
        BaseUrl = settings.BaseUrl
    };

    private static bool IsSuccess(string? status) =>
        string.Equals(status, "success", StringComparison.OrdinalIgnoreCase);

    // 1293.75m -> "1293.75" (Türkçe ayarda "1293,75" olurdu ve iyzico reddederdi)
    private static string ToIyzicoPrice(decimal amount) =>
        amount.ToString("0.00", CultureInfo.InvariantCulture);

    // "05071057599" -> "+905071057599"
    private static string ToGsmNumber(string phone) => "+90" + phone.TrimStart('0');

    // "Gokhan Kaya" -> ("Gokhan", "Kaya");  "Ali Veli Can" -> ("Ali Veli", "Can")
    private static (string Name, string Surname) SplitName(string fullName)
    {
        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 1
            ? (string.Join(' ', parts[..^1]), parts[^1])
            : (parts[0], parts[0]);
    }

    private static Address DemoAddressFor(string contactName) => new()
    {
        ContactName = contactName,
        City = DemoCity,
        Country = DemoCountry,
        Description = DemoAddress,
        ZipCode = DemoZipCode
    };
}