using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VirtualPos.Infrastructure.Persistence;

namespace VirtualPos.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<VirtualPosDbContext>(options =>
                options.UseSqlServer(connectionString));

            return services;
        }
    }
}
