using AuthECWebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthECWebApi.Controllers
{
    public static class IdentityUserEndpoints
    {
        public static IEndpointRouteBuilder MapIdentityUserEndpoints(this IEndpointRouteBuilder app)
        {
            // Minimal Post API for  User SignUp
            app.MapPost("/signup", CreateUser);

            // Minimal Post API for  User SignIn
            app.MapPost("/signin", SignInUser);

            return app;
        }
        [AllowAnonymous]
        private static async Task<IResult> CreateUser(UserManager<AppUser> userManager,
                [FromBody] UserRegistrationModel userRegistrationModel)
        {
            AppUser user = new AppUser()
            {
                UserName = userRegistrationModel.Email,
                Email = userRegistrationModel.Email,
                FullName = userRegistrationModel.FullName,
                Gender = userRegistrationModel.Gender,
                DOB = DateOnly.FromDateTime(DateTime.Now.AddYears(-userRegistrationModel.Age)),
                LibraryID = userRegistrationModel.LibraryID
            };

            // Creating User

            var result = await userManager.CreateAsync(user, userRegistrationModel.Password);

            // Assign role while creating
            await userManager.AddToRoleAsync(user, userRegistrationModel.Role);

            if (result.Succeeded)
                return Results.Ok(result);
            else
                return Results.BadRequest(result);
        }

        [AllowAnonymous]
        private static async Task<IResult> SignInUser(UserManager<AppUser> userManager,
                [FromBody] UserSignInModel userSignInModel, IOptions<AppSettings> appSettings)
        {
            // Validating user
            var user = await userManager.FindByEmailAsync(userSignInModel.Email);
            if (user != null && await userManager.CheckPasswordAsync(user, userSignInModel.Password))
            {
                // UserRoles
                var roles = await userManager.GetRolesAsync(user);

                // SignIn key for token validation
                var signInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Value.JWTSecretKey));

                // User Claims:
                ClaimsIdentity claims = new ClaimsIdentity(new Claim[]
                {
                    new Claim("UserID", user.Id.ToString()),
                    new Claim("Gender", user.Gender.ToString()),
                    new Claim("Age", (DateTime.Now.Year - user.DOB.Year).ToString()),
                    new Claim(ClaimTypes.Role,roles.First()),
                });

                if (user.LibraryID != null)
                    claims.AddClaim(new Claim("LibraryID", user.LibraryID.ToString()!));

                // Token descriptor
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = claims,
                    //Expires = DateTime.UtcNow.AddMinutes(10),
                    Expires = DateTime.UtcNow.AddDays(10),
                    SigningCredentials = new SigningCredentials(signInKey, SecurityAlgorithms.HmacSha256Signature)
                };

                // Create a token handler
                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);

                // Required Desearlized form of token
                var token = tokenHandler.WriteToken(securityToken);
                return Results.Ok(new { token });  // Token Structure: Header.Payload.Signature

            }
            else
                return Results.BadRequest(new { message = "Email or Password in Incorrect." });
        }
    }

    public class UserRegistrationModel
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public int Age { get; set; }
        public int? LibraryID { get; set; }
    }

    public class UserSignInModel
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
