using AuthECWebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AuthECWebApi.Controllers
{
    public static class AccountEndpoints
    {
        // For Authorization, both things can be used, either [Authorize] attribute or RequireAuthorization() method.
        public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder app)
        {
            // Minimal Post API for Authorization.
            app.MapGet("/UserProfile", GetUserProfile);
            //app.MapGet("/UserProfile", GetUserProfile).RequireAuthorization();
            return app;
        }
        [Authorize]
        private static async Task<IResult> GetUserProfile(ClaimsPrincipal user, UserManager<AppUser> userManager)
        {
            string userId = user.Claims.First(x => x.Type == "UserID").Value;
            var userDetails = await userManager.FindByIdAsync(userId);
            return Results.Ok(
                new
                {
                    Email = userDetails?.Email,
                    FullName = userDetails?.FullName,
                    Gender = userDetails?.Gender,
                    Age = (DateTime.Now.Year - userDetails?.DOB.Year).ToString()
                });
        }
    }

    //private static object GetUserProfile()
    //{
    //    return new
    //    {
    //        Name = "Secret Name",
    //        BankBalance = "$1000000000",
    //        CardDetails = "4242 4242 4242 4242",
    //    };
    //}
}
