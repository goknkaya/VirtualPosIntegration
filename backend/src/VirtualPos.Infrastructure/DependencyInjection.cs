using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VirtualPos.Application.Bins;
using VirtualPos.Application.Notifications;
using VirtualPos.Application.Orders;
using VirtualPos.Application.Payments;
using VirtualPos.Infrastructure.Bins;
using VirtualPos.Infrastructure.MockBank;
using VirtualPos.Infrastructure.Notifications;
using VirtualPos.Infrastructure.Payments;
using VirtualPos.Infrastructure.Persistence;

namespace VirtualPos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        string apiBaseUrl,
        string paymentProvider,
        IyzicoSettings? iyzicoSettings)
    {
        services.AddDbContext<VirtualPosDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IBinProvider, SqlBinProvider>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ISmsSender, ConsoleSmsSender>();

        // Sahte banka her zaman kayıtlı: şifre sayfası (MockBankController) bu hafızayı kullanıyor
        services.AddSingleton(new MockBankSettings(apiBaseUrl));
        services.AddSingleton<MockBankSessionStore>();

        // Ödeme sağlayıcısı ayardan seçilir: "MockBank" veya "Iyzico"
        if (paymentProvider.Equals("Iyzico", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton(iyzicoSettings
                ?? throw new InvalidOperationException("iyzico ayarları eksik."));
            services.AddScoped<IPaymentProvider, IyzicoPaymentProvider>();
        }
        else
        {
            services.AddScoped<IPaymentProvider, MockPaymentProvider>();
        }

        return services;
    }
}