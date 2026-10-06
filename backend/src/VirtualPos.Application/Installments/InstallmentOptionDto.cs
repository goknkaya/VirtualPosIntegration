using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualPos.Application.Installments
{
    public record InstallmentOptionDto(int Count, decimal TotalAmount, decimal MonthlyAmount);
}
