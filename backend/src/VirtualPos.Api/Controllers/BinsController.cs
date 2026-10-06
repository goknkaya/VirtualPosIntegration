using Microsoft.AspNetCore.Mvc;
using VirtualPos.Application.Bins;

namespace VirtualPos.Api.Controllers;

[ApiController]
[Route("api/bins")]
public class BinsController(IBinProvider binProvider) : ControllerBase
{
    [HttpGet("{binNumber}")]
    public async Task<IActionResult> Get(string binNumber, CancellationToken ct)
    {
        if (binNumber.Length is < 6 or > 8 || !binNumber.All(char.IsAsciiDigit))
            return BadRequest(new { message = "BIN 6-8 haneli bir sayı olmalıdır." });

        var info = await binProvider.GetBinInfoAsync(binNumber, ct);

        return info is null
            ? NotFound(new { message = "BIN bulunamadı." })
            : Ok(info);
    }
}