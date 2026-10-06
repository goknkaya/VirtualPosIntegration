using System;
using System.Collections.Generic;
using System.Text;
using VirtualPos.Domain.Enums;

namespace VirtualPos.Domain.Entities;

public class Order
{
    public long Id { get; set; }
    public Guid OrderNo { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public string BuyerPhone { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }

    public List<Payment> Payments { get; set; } = new();
}
