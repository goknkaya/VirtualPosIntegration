using System;
using System.Collections.Generic;
using System.Text;
using VirtualPos.Domain.Entities;
using VirtualPos.Domain.Enums;

namespace VirtualPos.Application.Orders;

public class OrderService(IOrderRepository repository)
{
    public const decimal MaxAmount = 1_000_000m;

    public async Task<CreateOrderResult> CreateAsync(CreateOrderRequest request, CancellationToken ct = default)
    {
        if (request.Amount <= 0 || request.Amount > MaxAmount)
            return Fail($"Tutar 0 ile {MaxAmount:N0} TL arasında olmalıdır.");

        if (decimal.Round(request.Amount, 2) != request.Amount)
            return Fail("Tutar en fazla 2 ondalık basamak içerebilir.");

        var phone = NormalizePhone(request.BuyerPhone);
        if (phone is null)
            return Fail("Telefon numarası 05XX XXX XX XX formatında olmalıdır.");

        var order = new Order
        {
            OrderNo = Guid.NewGuid(),
            Amount = request.Amount,
            Currency = "TRY",
            BuyerPhone = phone,
            Status = OrderStatus.AwaitingPayment,
            CreatedAt = DateTime.UtcNow
        };

        await repository.AddAsync(order, ct);
        await repository.SaveChangesAsync(ct);

        return new CreateOrderResult(true, ToDto(order), null);
    }

    public async Task<OrderDto?> GetAsync(Guid orderNo, CancellationToken ct = default)
    {
        var order = await repository.GetByOrderNoAsync(orderNo, ct);
        return order is null ? null : ToDto(order);
    }

    // "0507 105 75 99", "5071057599", "+90 507 105 75 99" -> "05071057599"
    public static string? NormalizePhone(string? input)
    {
        var digits = new string((input ?? string.Empty).Where(char.IsAsciiDigit).ToArray());

        if (digits.Length == 12 && digits.StartsWith("90"))
            digits = digits[2..];
        if (digits.Length == 10 && digits[0] == '5')
            digits = "0" + digits;

        return digits.Length == 11 && digits.StartsWith("05") ? digits : null;
    }

    // "05071057599" -> "0507*****99"
    public static string MaskPhone(string phone) =>
        phone.Length < 7 ? phone : $"{phone[..4]}{new string('*', phone.Length - 6)}{phone[^2..]}";

    private static OrderDto ToDto(Order o) =>
        new(o.OrderNo, o.Amount, o.Currency, MaskPhone(o.BuyerPhone), o.Status.ToString(), o.CreatedAt);

    private static CreateOrderResult Fail(string message) => new(false, null, message);
}
