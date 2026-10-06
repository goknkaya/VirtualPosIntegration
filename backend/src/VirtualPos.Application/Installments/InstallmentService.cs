using System;
using System.Collections.Generic;
using System.Text;
using VirtualPos.Application.Bins;

namespace VirtualPos.Application.Installments
{
    public class InstallmentService(IBinProvider binProvider)
    {
        // (taksit sayısı, vade farkı oranı) - örnek oranlar
        private static readonly (int Count, decimal Rate)[] Plans =
        [
            (1, 0m),
            (2, 0.025m),
            (3, 0.035m),
            (6, 0.06m),
            (9, 0.09m)
        ];

        public async Task<IReadOnlyList<InstallmentOptionDto>?> GetOptionsAsync(
            string bin, decimal amount, CancellationToken ct = default)
        {
            var binInfo = await binProvider.GetBinInfoAsync(bin, ct);
            if (binInfo is null)
                return null;

            // Kredi kartı değilse sadece tek çekim
            var plans = binInfo.IsCreditCard ? Plans : Plans[..1];

            return plans
                .Select(p =>
                {
                    var total = Math.Round(amount * (1 + p.Rate), 2);
                    return new InstallmentOptionDto(p.Count, total, Math.Round(total / p.Count, 2));
                })
                .ToList();
        }
    }
}
