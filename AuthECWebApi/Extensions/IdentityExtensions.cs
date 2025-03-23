using AuthECWebApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AuthECWebApi.Extensions
{
    public static class IdentityExtensions
    {
        public static IServiceCollection AddIdentityHandlersAndStores(this IServiceCollection services)
        {

            services.AddIdentityApiEndpoints<AppUser>()  // Identity Manager Layer
                    .AddEntityFrameworkStores<AppDbContext>();  // Identity Store Layer
            return services;
        }

        public static IServiceCollection ConfigureIdentityOptions(this IServiceCollection services)
        {
            // Custom Configuration for Identity Properties
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.User.RequireUniqueEmail = true;
            });
            return services;
        }

        //Auth = Authtication + Authorization
        public static IServiceCollection AddIdentityAuth(this IServiceCollection services, IConfiguration configuration)
        {
            // Adding Identity Authtication Service (can be any provider OAuth, JWT etc)    
            // AddJwtBearer is used to register schemes with necessary configurations.
            services.AddAuthentication(x =>
            {
                // This scheme contains core handlers related to authtication process.
                x.DefaultAuthenticateScheme =
                // This scheme contains core handlers related to unauthorized access requests.
                x.DefaultChallengeScheme =

                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(y =>
            {
                y.SaveToken = true;
                y.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                        configuration["AppSettings:JWTSecretKey"]!)) // ! vanish null error warnings.
                };
            });
            return services;
        }

        public static WebApplication AddIdentityAuthMiddlewares(this WebApplication app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
            return app;
        }
    }
}
