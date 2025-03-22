using AuthECWebApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Text;

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

// Adding Identity Authtication Service (can be any provider OAuth, JWT etc)    
builder.Services.AddAuthentication(x =>
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
                    builder.Configuration["AppSettings:JWTSecretKey"]!)) // ! vanish null error warnings.
            };
        });
// AddJwtBearer is used to register schemes with necessary configurations.


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS, Authentication, Authorization middle ware must be in same sequence
#region Config.CORS
app.UseCors();
#endregion
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Add corresponsing routing for indentity 
app.MapGroup("/api")
   .MapIdentityApi<AppUser>();

// Minimal Post API for  User SignUp
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


// Minimal Post API for  User SignIn
app.MapPost("/api/signin", async (UserManager<AppUser> userManager,
    [FromBody] UserSignInModel userSignInModel) =>
{
    // Validating user
    var user = await userManager.FindByEmailAsync(userSignInModel.Email);
    if(user != null && await userManager.CheckPasswordAsync(user, userSignInModel.Password))
    {
        // SignIn key for token validation
        var signInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                    builder.Configuration["AppSettings:JWTSecretKey"]!));

        // Token descriptor
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim("UserID", user.Id.ToString())
            }),
            //Expires = DateTime.UtcNow.AddMinutes(10),
            Expires = DateTime.UtcNow.AddDays(10),
            SigningCredentials = new SigningCredentials(signInKey, SecurityAlgorithms.HmacSha256Signature)
        };

        // Create a token handler
        var tokenHandler  = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        
        // Required Desearlized form of token
        var token = tokenHandler.WriteToken(securityToken);
        return Results.Ok(new {token});  // Token Structure: Header.Payload.Signature

    }
    else
        return Results.BadRequest(new { message = "Email or Password in Incorrect." });
});

app.Run();

public class UserRegistrationModel
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FullName { get; set; } = null!;
}

public class UserSignInModel
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

