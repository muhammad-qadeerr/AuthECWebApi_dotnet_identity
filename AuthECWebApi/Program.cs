using AuthECWebApi.Controllers;
using AuthECWebApi.Extensions;
using AuthECWebApi.Models;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddSwaggerExplorer()
                .InjectDbContext(builder.Configuration)
                .AddAppConfig(builder.Configuration)
                .AddIdentityHandlersAndStores()
                .ConfigureIdentityOptions()
                .AddIdentityAuth(builder.Configuration);


var app = builder.Build();

// CORS, Authentication, Authorization middle ware must be in same sequence
app.ConfigureSwaggerExplorer()
    .ConfigureCORS(builder.Configuration)
    .AddIdentityAuthMiddlewares();

app.MapControllers();

// Add corresponsing routing for indentity 
app.MapGroup("/api")
   .MapIdentityApi<AppUser>();
app.MapGroup("/api")
    .MapIdentityUserEndpoints();

app.Run();