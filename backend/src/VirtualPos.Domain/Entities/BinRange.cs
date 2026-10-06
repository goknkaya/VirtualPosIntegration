using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualPos.Domain.Entities
{
    public class BinRange
    {
        public int BinRangeMin { get; set; }
        public int BinRangeMax { get; set; }
        public string? CardType { get; set; }
        public string? CardTypeCode { get; set; }
        public string? BankEftCode { get; set; }
        public string? BankName { get; set; }
        public string? BrandName { get; set; }
        public int? CardNoLength { get; set; }
        public bool IsMastercard { get; set; }
        public bool IsVisa { get; set; }
        public bool IsTroy { get; set; }
        public bool IsAmex { get; set; }
        public bool IsCreditCard { get; set; }
        public bool IsDebitCard { get; set; }
        public bool IsPrepaidCard { get; set; }
    }
}
