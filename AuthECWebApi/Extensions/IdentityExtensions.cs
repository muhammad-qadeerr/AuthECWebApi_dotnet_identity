using AuthECWebApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
                    .AddRoles<IdentityRole>()  // Identity Role Layer
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
            // We can set the required schemes ourselves but this can directly be used if not: JwtBearerDefaults.AuthenticationScheme
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(y =>
            {
                y.SaveToken = true;
                y.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                        configuration["AppSettings:JWTSecretKey"]!)), // ! vanish null error warnings.

                    // Validate Issuer and audience if you written while creating token, otherwise set to false
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            // Adding Indentity Authorization Service

            services.AddAuthorization(options =>
            {
                // This add Authorization to all the web api methods
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();

                // Adding Authorization Policies
                options.AddPolicy("HasLibraryID", policy => policy.RequireClaim("LibraryID"));
                options.AddPolicy("FemalesOnly", policy => policy.RequireClaim("Gender","Female"));  // RequireClaim(propertyName, Value1, Value2) we can pass multiple values also.
                options.AddPolicy("AgeUnder10", policy => policy.RequireAssertion(context =>
                Int32.Parse(context.User.Claims.First(x => x.Type == "Age").Value) < 10));

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
