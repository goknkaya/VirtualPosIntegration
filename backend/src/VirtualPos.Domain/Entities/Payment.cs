using VirtualPos.Domain.Enums;

namespace VirtualPos.Domain.Entities;

public class Payment
{
    public long Id { get; set; }
    public Guid PaymentNo { get; set; }

    public long OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public byte Installment { get; set; } = 1;
    public string CardHolderName { get; set; } = string.Empty;
    public string MaskedCardNumber { get; set; } = string.Empty;
    public string BinNumber { get; set; } = string.Empty;
    public string? BankEftCode { get; set; }
    public string? BankName { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string? ProviderPaymentId { get; set; }
    public PaymentStatus Status { get; set; }
    public string? MdStatus { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<PaymentLog> Logs { get; set; } = new();
}