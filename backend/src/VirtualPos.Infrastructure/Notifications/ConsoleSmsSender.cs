using Microsoft.Extensions.Logging;
using VirtualPos.Application.Notifications;
using VirtualPos.Application.Orders;

namespace VirtualPos.Infrastructure.Notifications;

public class ConsoleSmsSender(ILogger<ConsoleSmsSender> logger) : ISmsSender
{
    public Task SendAsync(string phoneNumber, string message, CancellationToken ct = default)
    {
        logger.LogInformation("[SMS] {Phone} numarasına gönderildi: {Message}",
            OrderService.MaskPhone(phoneNumber), message);
        return Task.CompletedTask;
    }
}