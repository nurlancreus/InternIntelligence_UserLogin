using InternIntelligence_UserLogin.Core.Abstractions;
using InternIntelligence_UserLogin.Core.DTOs.Auth;
using InternIntelligence_UserLogin.Validators;
using Microsoft.AspNetCore.Mvc;

namespace InternIntelligence_UserLogin.Endpoints
{
    public static class Auth
    {
        public static IEndpointRouteBuilder RegisterAuthEndpoints(this IEndpointRouteBuilder routes)
        {
            var auth = routes.MapGroup("api/auth");

            auth.MapPost("register", async (IAuthService authService, [FromBody] RegisterDTO registerDto) =>
            {
                await authService.RegisterAsync(registerDto);
            }).Validate<RegisterDTO>();

            auth.MapPost("login", async (IAuthService authService, [FromBody] LoginDTO loginDto) =>
            {
                var tokenDto = await authService.LoginAsync(loginDto);

                return Results.Ok(tokenDto);
            }).Validate<LoginDTO>();

            auth.MapPost("refresh-login", async (IAuthService authService, [FromBody] RefreshLoginDTO refreshLoginDTO) =>
            {
                var tokenDto = await authService.RefreshLoginAsync(refreshLoginDTO);

                return Results.Ok(tokenDto);
            }).Validate<LoginDTO>();

            auth.MapPatch("confirm-email", async (IAuthService authService, [FromQuery] string userId, [FromQuery] string token) =>
            {
                await authService.ConfirmEmailAsync(userId, token);

                return Results.Ok();
            });

            auth.MapGet("{id}/reset-password", async (string id, IAuthService authService) =>
            {
                await authService.RequestPasswordResetAsync(id);

                return Results.Ok();
            });

            auth.MapPatch("reset-password", async (IAuthService authService, [FromQuery] string userId, [FromQuery] string token, [FromBody] string newPassword) =>
            {
                await authService.ResetPasswordAsync(userId, token, newPassword);

                return Results.Ok();
            });

            auth.MapPost("logout", async (IAuthService authService) =>
            {

            });

            return routes;
        }
    }
}
