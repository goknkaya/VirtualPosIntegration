namespace VirtualPos.Application.Payments;

// Vue'dan gelen ödeme isteği: tutar yok, hangi siparişin ödendiği var
public record InitializePaymentRequest(
    Guid OrderNo,
    string CardHolderName,
    string CardNumber,
    string ExpireMonth,
    string ExpireYear,
    string Cvc,
    int Installment);

// Vue'ya dönen cevap: kullanıcı nereye yönlendirilecek
public record InitializePaymentResult(
    bool IsSuccess,
    Guid? PaymentNo,
    string? RedirectUrl,
    string? HtmlContent,
    string? ErrorMessage);

// Bankanın callback adresimize form ile gönderdiği alanlar
public class ThreeDsCallback
{
    public string? Status { get; set; }
    public string? PaymentId { get; set; }
    public string? ConversationId { get; set; }
    public string? MdStatus { get; set; }
}

// Sonuç sayfası için ödeme bilgisi
public record PaymentResultDto(
    Guid PaymentNo,
    Guid OrderNo,
    string Status,
    decimal Amount,
    int Installment,
    string MaskedCardNumber,
    string? BankName,
    string? ErrorMessage,
    DateTime CreatedAt);

// API ve Vue'nun adresleri
public record PaymentSettings(string ApiBaseUrl, string FrontendBaseUrl);