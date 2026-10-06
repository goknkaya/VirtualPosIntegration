using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualPos.Application.Payments;

// Vue'dan gelen ödeme isteği (tutar yok: sunucuda belirlenir)
public record InitializePaymentRequest(
    string CardHolderName,
    string CardNumber,
    string ExpireMonth,
    string ExpireYear,
    string Cvc,
    int Installment);

// Vue'ya dönen cevap: kullanıcı nereye yönlendirilecek
public record InitializePaymentResult(
    bool IsSuccess,
    Guid? OrderNo,
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
