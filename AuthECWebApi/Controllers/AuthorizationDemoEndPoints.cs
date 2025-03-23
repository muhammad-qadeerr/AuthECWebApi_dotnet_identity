using Microsoft.AspNetCore.Authorization;

namespace AuthECWebApi.Controllers
{
    public static class AuthorizationDemoEndPoints
    {
        public static IEndpointRouteBuilder MapAuthorizationDemoEndPoints(this IEndpointRouteBuilder app)
        {
            // Minimal Post API for Authorization.
            app.MapGet("/AdminOnly", AdminOnly);
            app.MapGet("/AdminOrTeacher", AdminOrTeacher);
            app.MapGet("/LibraryMemberOnly", AccessLibrary);

            // Instead of writting seperate method it can be done using lambda expressions for now.
            // Teacher and Female Only
            app.MapGet("/ApplyMaternityLeave", [Authorize(Roles = "Teacher", Policy ="FemalesOnly")] () =>
            {
                return new { AccessMessage = "Leave Applied Successfully." };
            });

            // Age Under 10 only
            app.MapGet("/AgeUnder10", [Authorize(Policy = "AgeUnder10")] () =>
            {
                return new { Access = "Under10 Only" };
            });

            // Age Under 10 and Females only
            app.MapGet("/AgeUnder10AndFemales", [Authorize(Policy = "AgeUnder10")][Authorize(Policy = "FemalesOnly")] () =>
            {
                return new { Access = "Under10 and Females Only" };
            });

            return app;
        }

        [Authorize(Roles = "Admin")]
        private static object AdminOnly()
        {
            return new { Access = "Admin Only" };
        }
        [Authorize(Roles = "Admin,Teacher")]
        private static object AdminOrTeacher()
        {
            return new { Access = "Admin Or Teacher" };
        }
        [Authorize(Policy = "HasLibraryID")]
        private static object AccessLibrary()
        {
            return new { Access = "Has LibraryId Policy" };
        }
    }
}
