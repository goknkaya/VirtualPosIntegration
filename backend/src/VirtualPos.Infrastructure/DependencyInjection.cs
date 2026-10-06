using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VirtualPos.Infrastructure.Persistence;
using VirtualPos.Application.Bins;
using VirtualPos.Infrastructure.Bins;

namespace VirtualPos.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<VirtualPosDbContext>(options =>
                options.UseSqlServer(connectionString));
            
            services.AddScoped<IBinProvider, SqlBinProvider>();

            return services;
        }
    }
}
