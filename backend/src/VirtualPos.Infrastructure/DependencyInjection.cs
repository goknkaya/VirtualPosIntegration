using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VirtualPos.Application.Bins;
using VirtualPos.Application.Notifications;
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
        this IServiceCollection services, string connectionString, string apiBaseUrl)
    {
        services.AddDbContext<VirtualPosDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IBinProvider, SqlBinProvider>();

        // Sahte banka
        services.AddSingleton(new MockBankSettings(apiBaseUrl));
        services.AddSingleton<MockBankSessionStore>();
        services.AddScoped<ISmsSender, ConsoleSmsSender>();
        services.AddScoped<IPaymentProvider, MockPaymentProvider>();

        return services;
    }
}