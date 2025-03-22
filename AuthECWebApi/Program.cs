using AuthECWebApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adding Services from .NET Identity
builder.Services
    .AddIdentityApiEndpoints<AppUser>()  // Identity Manager Layer
    .AddEntityFrameworkStores<AppDbContext>();  // Identity Store Layer

// Injecting instance of DB Context class (From EF Core)
builder.Services.AddDbContext<AppDbContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("DevDB")));

// Custom Configuration for Identity Properties
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

// Add corresponsing routing for indentity 
app.MapGroup("/api")
   .MapIdentityApi<AppUser>();

// Minimal Post API for tesintg register user endpoint
app.MapPost("/api/signup", async (UserManager<AppUser> userManager,
    [FromBody] UserRegistrationModel userRegistrationModel) =>
{
    AppUser user = new AppUser() 
    { 
        UserName = userRegistrationModel.Email,
        Email = userRegistrationModel.Email,
        FullName = userRegistrationModel.FullName
    };

    // Creating User

    var result = await userManager.CreateAsync(user, userRegistrationModel.Password);

    if (result.Succeeded)
        return Results.Ok(result);
    else
        return Results.BadRequest(result);

});

app.Run();

public class UserRegistrationModel
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FullName { get; set; } = null!;
}

