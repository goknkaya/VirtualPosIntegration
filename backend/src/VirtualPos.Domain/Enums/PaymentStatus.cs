using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualPos.Domain.Enums
{
    public enum PaymentStatus : byte
    {
        Pending3DS = 0,
        Succeeded = 1,
        Failed = 2,
        Cancelled = 3
    }
}
