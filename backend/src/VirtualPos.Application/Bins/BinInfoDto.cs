using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualPos.Application.Bins
{
    public record BinInfoDto(
    string Bin,
    string BankName,
    string? BankEftCode,
    string CardScheme,
    string CardType,
    string? BrandName);
}
