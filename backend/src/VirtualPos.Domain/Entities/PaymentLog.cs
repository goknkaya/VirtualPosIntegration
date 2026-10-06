using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualPos.Domain.Entities
{
    public class PaymentLog
    {
        public long Id { get; set; }
        public long PaymentId { get; set; }
        public Payment Payment { get; set; } = null!;
        public string Step { get; set; } = string.Empty;
        public string? RequestJson { get; set; }
        public string? ResponseJson { get; set; }
        public bool IsSuccess { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
