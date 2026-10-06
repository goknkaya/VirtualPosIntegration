using Microsoft.EntityFrameworkCore;
using VirtualPos.Application.Bins;
using VirtualPos.Domain.Entities;
using VirtualPos.Infrastructure.Persistence;

namespace VirtualPos.Infrastructure.Bins;

public class SqlBinProvider(VirtualPosDbContext db) : IBinProvider
{
    public async Task<BinInfoDto?> GetBinInfoAsync(string bin, CancellationToken ct = default)
    {
        // 6 haneli 454671 -> 45467100 ile 45467199 arası
        int factor = (int)Math.Pow(10, 8 - bin.Length);
        int rangeStart = int.Parse(bin) * factor;
        int rangeEnd = rangeStart + factor - 1;

        var row = await db.BinRanges
            .AsNoTracking()
            .Where(b => b.BinRangeMin <= rangeEnd && b.BinRangeMax >= rangeStart)
            .OrderBy(b => b.BinRangeMax - b.BinRangeMin)
            .FirstOrDefaultAsync(ct);

        if (row is null)
            return null;

        return new BinInfoDto(
            bin,
            row.BankName ?? "Bilinmeyen banka",
            row.BankEftCode,
            GetScheme(row),
            GetCardType(row),
            row.BrandName);
    }

    private static string GetScheme(BinRange b) =>
        b.IsVisa ? "Visa" :
        b.IsMastercard ? "Mastercard" :
        b.IsTroy ? "Troy" :
        b.IsAmex ? "Amex" : "Diğer";

    private static string GetCardType(BinRange b) =>
        b.IsCreditCard ? "Kredi Kartı" :
        b.IsDebitCard ? "Banka Kartı" :
        b.IsPrepaidCard ? "Ön Ödemeli Kart" : "Bilinmiyor";
}