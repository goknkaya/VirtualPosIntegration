using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualPos.Application.Bins
{
    public interface IBinProvider
    {
        Task<BinInfoDto?> GetBinInfoAsync(string bin, CancellationToken ct = default);
    }
}
