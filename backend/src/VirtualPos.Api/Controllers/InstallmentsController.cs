using Microsoft.AspNetCore.Mvc;
using VirtualPos.Application.Installments;

namespace VirtualPos.Api.Controllers
{
    [ApiController]
    [Route("api/installments")]
    public class InstallmentsController(InstallmentService installmentService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string bin, [FromQuery] decimal amount, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(bin) || bin.Length is < 6 or > 8 || !bin.All(char.IsAsciiDigit))
                return BadRequest(new { message = "BIN 6-8 haneli bir sayı olmalıdır." });

            if (amount <= 0)
                return BadRequest(new { message = "Tutar sıfırdan büyük olmalıdır." });

            var options = await installmentService.GetOptionsAsync(bin, amount, ct);

            return options is null
                ? NotFound(new { message = "BIN bulunamadı." })
                : Ok(options);
        }
    }
}
