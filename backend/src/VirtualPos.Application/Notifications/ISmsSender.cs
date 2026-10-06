using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualPos.Application.Notifications;

public interface ISmsSender
{
    Task SendAsync(string phoneNumber, string message, CancellationToken ct = default);
}
