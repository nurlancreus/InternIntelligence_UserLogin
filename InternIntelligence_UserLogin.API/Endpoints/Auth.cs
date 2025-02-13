using InternIntelligence_UserLogin.API.Validators;
using InternIntelligence_UserLogin.Core.Abstractions.Services;
using InternIntelligence_UserLogin.Core.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;

namespace InternIntelligence_UserLogin.API.Endpoints
{
    public static class Auth
    {
        public static IEndpointRouteBuilder RegisterAuthEndpoints(this IEndpointRouteBuilder routes)
        {
            var auth = routes.MapGroup("api/auth").AllowAnonymous();

            auth.MapPost("register", async (IAuthService authService, [FromBody] RegisterDTO registerDto) =>
            {
                var userId = await authService.RegisterAsync(registerDto);

                return Results.Ok(userId);
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
            }).Validate<RefreshLoginDTO>();

            auth.MapPatch("confirm-email", async (IAuthService authService, [FromQuery] Guid userId, [FromQuery] string token) =>
            {
                await authService.ConfirmEmailAsync(userId, token);

                return Results.Ok();
            });

            auth.MapPost("logout", async (IAuthService authService) =>
            {

            }).RequireAuthorization();

            return routes;
        }
    }
}
