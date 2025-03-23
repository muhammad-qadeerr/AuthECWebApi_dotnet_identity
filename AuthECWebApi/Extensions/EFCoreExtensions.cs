using AuthECWebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthECWebApi.Extensions
{
    public static class EFCoreExtensions
    {
        public static IServiceCollection InjectDbContext(this IServiceCollection services, IConfiguration config)
        {
            // Injecting instance of DB Context class (From EF Core)
            services.AddDbContext<AppDbContext>(options =>
                   options.UseSqlServer(config.GetConnectionString("DevDB")));
            return services;
        }
    }
}
