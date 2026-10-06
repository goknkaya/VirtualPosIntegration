using Microsoft.AspNetCore.Mvc;
using VirtualPos.Application.Orders;

namespace VirtualPos.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController(OrderService orderService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var result = await orderService.CreateAsync(request, ct);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return CreatedAtAction(nameof(Get), new { orderNo = result.Order!.OrderNo }, result.Order);
    }

    [HttpGet("{orderNo:guid}")]
    public async Task<IActionResult> Get(Guid orderNo, CancellationToken ct)
    {
        var order = await orderService.GetAsync(orderNo, ct);
        return order is null
            ? NotFound(new { message = "Sipariş bulunamadı." })
            : Ok(order);
    }
}
