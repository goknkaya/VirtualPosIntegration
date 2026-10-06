using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualPos.Domain.Enums
{
    public enum OrderStatus : byte
    {
        AwaitingPayment = 0,
        Paid = 1
    }
}
