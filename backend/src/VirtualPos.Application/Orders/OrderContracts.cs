using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualPos.Application.Orders;

// Sipariş oluşturma isteği
public record CreateOrderRequest(decimal Amount, string BuyerPhone);

// Dışarıya gösterilen sipariş bilgisi (telefon maskeli)
public record OrderDto(
    Guid OrderNo,
    decimal Amount,
    string Currency,
    string BuyerPhoneMasked,
    string Status,
    DateTime CreatedAt);

public record CreateOrderResult(bool IsSuccess, OrderDto? Order, string? ErrorMessage);
