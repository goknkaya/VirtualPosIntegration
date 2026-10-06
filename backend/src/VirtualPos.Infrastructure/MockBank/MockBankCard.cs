using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualPos.Infrastructure.MockBank
{
    // Sahte bankanın kart kaydı: gerçekte bu tablo bankanın kendi sisteminde durur
    public class MockBankCard
    {
        public int Id { get; set; }
        public string CardHash { get; set; } = string.Empty;
        public string MaskedCardNumber { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }
}
