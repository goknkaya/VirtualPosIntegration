using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualPos.Application.Payments
{
    // Sağlayıcıya gönderilen 3DS başlatma isteği
    public record ThreeDsInitRequest(
        Guid OrderNo,
        decimal Amount,
        string Currency,
        int Installment,
        string CardHolderName,
        string CardNumber,
        string ExpireMonth,
        string ExpireYear,
        string Cvc,
        string CallbackUrl);

    // Sağlayıcının başlatma cevabı: kullanıcıyı ya bir adrese yönlendir ya da HTML göster
    public record ThreeDsInitResult(
        bool IsSuccess,
        string? ProviderPaymentId,
        string? RedirectUrl,
        string? HtmlContent,
        string? ErrorCode,
        string? ErrorMessage);

    // Sağlayıcının tamamlama (provizyon) cevabı
    public record ThreeDsCompleteResult(bool IsSuccess, string? ErrorCode, string? ErrorMessage);
}
