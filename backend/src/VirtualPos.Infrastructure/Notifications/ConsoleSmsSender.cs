using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using VirtualPos.Application.Notifications;

namespace VirtualPos.Infrastructure.Notifications
{
    public class ConsoleSmsSender(ILogger<ConsoleSmsSender> logger) : ISmsSender
    {
        public Task SendAsync(string phoneNumber, string message, CancellationToken ct = default)
        {
            logger.LogInformation("[SMS] {Phone} numarasına gönderildi: {Message}", phoneNumber, message);
            return Task.CompletedTask;
        }
    }
}
