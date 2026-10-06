using Microsoft.AspNetCore.Mvc;
using VirtualPos.Application.Payments;

namespace VirtualPos.Api.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController(PaymentService paymentService, PaymentSettings settings) : ControllerBase
{
    [HttpPost("3ds/initialize")]
    public async Task<IActionResult> Initialize([FromBody] InitializePaymentRequest request, CancellationToken ct)
    {
        var result = await paymentService.InitializeAsync(request, ct);
        return result.IsSuccess
            ? Ok(result)
            : BadRequest(new { message = result.ErrorMessage });
    }

    // Banka sayfası tarayıcıyı buraya form ile gönderir (JSON değil)
    [HttpPost("3ds/callback")]
    public async Task<IActionResult> Callback([FromForm] ThreeDsCallback callback, CancellationToken ct)
    {
        var paymentNo = await paymentService.HandleCallbackAsync(callback, ct);
        return Redirect($"{settings.FrontendBaseUrl}/result/{paymentNo?.ToString() ?? "unknown"}");
    }

    [HttpGet("{paymentNo:guid}")]
    public async Task<IActionResult> Get(Guid paymentNo, CancellationToken ct)
    {
        var payment = await paymentService.GetAsync(paymentNo, ct);
        return payment is null
            ? NotFound(new { message = "Ödeme bulunamadı." })
            : Ok(payment);
    }
}