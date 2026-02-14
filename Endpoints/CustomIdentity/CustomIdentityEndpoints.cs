using CBCMissionaryWallApi.Endpoints.CustomIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Text;

namespace CBCMissionaryWallApi.Endpoints.CustomIdentity
{
    public static class CustomIdentityEndpoints
    {

        public static IEndpointRouteBuilder MapCustomIdentityEndpoints(this IEndpointRouteBuilder route)
        {
            //Step make a group
            var group = route.MapGroup("/api/auth")
               .WithTags("Admin");

            //step two endpoint
            group.MapPost("/register-admin", RegisterUser)
                .WithName("RegisterAdmin")
                .WithSummary("Register a User")
                .WithDescription("Registers a user. User must have admin role.")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest);

            group.MapPost("/reset-password", ResetPassword)
                .WithName("ResetPassword")
                .WithDescription("Custom Reset Password for a user")
                .WithSummary("Custom Reset Password")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest);

            group.MapPost("/forgot-password", ForgotPassword)
                .WithName("ForgotPassword")
                .WithDescription("Custom Forgot Password flow")
                .WithSummary("Custom Forgot Password")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest);


            group.MapGet("/manage/profile", GetProfileInfo)
                 .WithName("GetProfile")
                 .WithDescription("Get current user profile info")
                 .WithSummary("Custom the current user profile")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status404NotFound)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .RequireAuthorization();

            group.MapPut("/manage/profile", UpdateProfileInfo)
                 .WithName("UpdateProfile")
                 .WithDescription("Update the current profile")
                 .WithSummary("Allows user to update the current user profile")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status404NotFound)
                 .Produces(StatusCodes.Status401Unauthorized)
                 .RequireAuthorization();

            group.MapGet("/manage/users", ListAllUsers)
                 .WithName("ListUsers")
                 .WithDescription("List All Users")
                 .WithSummary("Lists all registered users")
                 .RequireAuthorization()
                 .Produces<IEnumerable<UserProfileResponse>>(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status401Unauthorized);


            return route;
        }

        private static async Task<IResult> RegisterUser(RegisterUserRequest request,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender,
            IConfiguration config)
        {

            //see if useremail sent in already exists
            if (await userManager.FindByEmailAsync(request.Email) is not null)
            {
                return Results.BadRequest(new { Error = $"User with email {request.Email} already exists." });
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName
            };

            var tempPassword = "TempPassword123!"; //Meet the password requirements  (generate in real world).

            var created = await userManager.CreateAsync(user, tempPassword);

            if (!created.Succeeded)
            {
                return Results.BadRequest(new { Error = created.Errors });
            }

            if (await roleManager.RoleExistsAsync("User"))
            {
                await userManager.AddToRoleAsync(user, "User");
            }

            //generate password reset token
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // send email to user to change password
            var baseUrl = config["BaseURL"] ?? "https://localhost:7008";
            var setPasswordLink = $"{baseUrl}/register-admin.html?email={user.Email}&resetCode={encodedToken}";

            await emailSender.SendEmailAsync(
                request.Email!,
                "Welcome to the CBC Missionary Wall Registry",
                $"""
                Your account has been created. Please change your password by visiting: 

                {setPasswordLink}

                """
                );

            return Results.Ok(new { Message = $"User {user.Email} created. Password reset link sent." });
        }

        private static async Task<IResult> ResetPassword(
             ResetPasswordRequest request,
             UserManager<ApplicationUser> userManager)
        {
            if (string.IsNullOrEmpty(request.Email) ||
                string.IsNullOrEmpty(request.ResetCode) ||
                string.IsNullOrEmpty(request.NewPassword))
            {
                return Results.BadRequest(new { Error = "All Fields are Required!" });
            }

            //find the user
            var user = await userManager.FindByEmailAsync(request.Email);

            if (user is null)
            {
                return Results.BadRequest(new { Error = "User not found!" });
            }

            try
            {
                var decodedToken = Encoding.UTF8.GetString(
                    WebEncoders.Base64UrlDecode(request.ResetCode));

                var result = await userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);

                if (result.Succeeded)
                {
                    return Results.Ok(new { Message = "Password reset successful!" });
                }

                return Results.BadRequest(new { Message = "Error" });
            }
            catch (FormatException)
            {
                return Results.BadRequest(new { Message = "Invalid Token" });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { Message = $"Error: {ex.Message}" });
            }
        }

        private static async Task<IResult> ForgotPassword(ForgotPasswordRequest request,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            IConfiguration config)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return Results.BadRequest(new { Message = "Email address is required." });
            }

            var user = await userManager.FindByEmailAsync(request.Email);

            if (user is null)
            {
                return Results.Ok(new { Message = "If the user exists, a forgot password link will be sent." });
            }

            //generate password reset token
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // send email to user to change password
            var baseUrl = config["BaseURL"] ?? "https://localhost:7008";
            var resetLink = $"{baseUrl}/reset-password.html?email={user.Email}&resetCode={encodedToken}";

            await emailSender.SendEmailAsync(
                request.Email!,
                "Reset Your Password",
                $"""
                To reset your password, use the link:

                {resetLink}

                """
                );

            return Results.Ok(new { Message = "If the user exists, a forgot password link will be sent." });
        }

        private static async Task<IResult> GetProfileInfo(ClaimsPrincipal principal,
            UserManager<ApplicationUser> userManager)
        {
            var user = await userManager.GetUserAsync(principal);

            if (user is null)
            {
                return Results.NotFound();
            }

            var response = new UserProfileResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                FullName = user.FullName
            };

            return Results.Ok(response);
        }


        private static async Task<IResult> UpdateProfileInfo(UpdateUserProfileRequest request,
            ClaimsPrincipal principal,
            UserManager<ApplicationUser> userManager)
        {
            //validate Inputs
            if (string.IsNullOrEmpty(request.FirstName) ||
                 string.IsNullOrEmpty(request.LastName))
            {
                return Results.BadRequest(new { Error = "First and Last Name are Required!" });
            }

            var user = await userManager.GetUserAsync(principal);

            if (user is null)
            {
                return Results.NotFound();
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;

            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return Results.BadRequest(new { Message = $"Update Failed: {result.Errors}" });
            }

            return Results.Ok(new { Message = "Profile updated successfully!" });

        }

        private static async Task<IResult> ListAllUsers(UserManager<ApplicationUser> userManager)
        {
            var users = userManager.Users
                .Select(u => new UserProfileResponse
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    FullName = u.FullName,
                    Email = u.Email
                }).ToList();


            return Results.Ok(users);
        }

    }
}
