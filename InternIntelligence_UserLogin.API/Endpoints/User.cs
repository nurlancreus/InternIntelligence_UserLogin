﻿using InternIntelligence_UserLogin.Core.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

namespace InternIntelligence_UserLogin.API.Endpoints
{
    public static class User
    {
        public static IEndpointRouteBuilder RegisterUserEndpoints(this IEndpointRouteBuilder routes)
        {
            var user = routes.MapGroup("api/users");

            user.MapGet("", async (IUserService userService, CancellationToken cancellationToken) =>
            {
                var users = await userService.GetAllAsync(cancellationToken);

                return Results.Ok(users);
            }).RequireAuthorization(ApiConstants.AuthPolicies.AdminsPolicy);

            user.MapGet("{id}", async ([FromRoute] Guid id, IUserService userService, CancellationToken cancellationToken) =>
            {
                var user = await userService.GetAsync(id, cancellationToken);

                return Results.Ok(user);
            }).RequireAuthorization(ApiConstants.AuthPolicies.UserOrAdminPolicy);

            user.MapGet("{id}/roles", async ([FromRoute] Guid id, IUserService userService, CancellationToken cancellationToken) =>
            {
                var roles = await userService.GetUserRoles(id, cancellationToken);

                return Results.Ok(roles);
            }).RequireAuthorization(ApiConstants.AuthPolicies.AdminsPolicy);

            user.MapPatch("{id}/assign-roles", async ([FromRoute] Guid id, [FromBody] IEnumerable<Guid> roleIds, IUserService userService, CancellationToken cancellationToken) =>
            {
                await userService.AssignRolesToUserAsync(id, roleIds, cancellationToken);

                return Results.Ok();
            }).RequireAuthorization(ApiConstants.AuthPolicies.SuperAdminPolicy);

            user.MapGet("{id}/reset-password", async (Guid id, IUserService userService) =>
            {
                await userService.RequestPasswordResetAsync(id);

                return Results.Ok();
            }).RequireAuthorization(ApiConstants.AuthPolicies.UserPolicy);

            user.MapPatch("reset-password", async (IUserService userService, [FromQuery] Guid userId, [FromQuery] string token, [FromBody] string newPassword) =>
            {
                await userService.ResetPasswordAsync(userId, token, newPassword);

                return Results.Ok();
            }).RequireAuthorization(ApiConstants.AuthPolicies.UserPolicy);

            return routes;
        }
    }
}
