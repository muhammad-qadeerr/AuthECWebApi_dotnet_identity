using AuthECWebApi.Models;

namespace AuthECWebApi.Extensions
{
    public static class AppConfigExtensions
    {
        public static WebApplication ConfigureCORS(this WebApplication app, IConfiguration config)
        {
            app.UseCors();
            return app;
        }

        public static IServiceCollection AddAppConfig(this IServiceCollection services, IConfiguration config)
        {
            // Injecting AppSettings Class
            services.Configure<AppSettings>(config.GetSection("AppSettings"));
            return services;
        }
    }
}
