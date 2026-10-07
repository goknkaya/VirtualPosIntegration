using VirtualPos.Application.Payments;

namespace VirtualPos.Api.BackgroundJobs;

// Uygulama açıkken her dakika çalışır, yarım kalan ödemeleri zaman aşımına uğratır
public class PaymentTimeoutJob(IServiceScopeFactory scopeFactory, ILogger<PaymentTimeoutJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                // Arka plan işi uygulama boyunca tek; DbContext ise her işlemde yeni olmalı
                using var scope = scopeFactory.CreateScope();
                var paymentService = scope.ServiceProvider.GetRequiredService<PaymentService>();

                var count = await paymentService.ExpirePendingPaymentsAsync(stoppingToken);
                if (count > 0)
                    logger.LogInformation("[Timeout] {Count} ödeme zaman aşımına uğradı.", count);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Zaman aşımı kontrolü başarısız oldu.");
            }
        }
    }
}
