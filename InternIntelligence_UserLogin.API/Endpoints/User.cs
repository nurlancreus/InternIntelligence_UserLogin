using InternIntelligence_UserLogin.Core.Abstractions.Services;
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
            });

            user.MapGet("{id}", async ([FromRoute] Guid id, IUserService userService, CancellationToken cancellationToken) =>
            {
                var user = await userService.GetAsync(id, cancellationToken);

                return Results.Ok(user);
            });

            user.MapGet("{id}/roles", async ([FromRoute] Guid id, IUserService userService, CancellationToken cancellationToken) =>
            {
                var roles = await userService.GetUserRoles(id, cancellationToken);

                return Results.Ok(roles);
            });

            user.MapPatch("{id}/assign-roles", async ([FromRoute] Guid id, [FromBody] IEnumerable<Guid> roleIds, IUserService userService, CancellationToken cancellationToken) =>
            {
                await userService.AssignRolesToUserAsync(id, roleIds, cancellationToken);

                return Results.Ok();
            });

            return routes;
        }
    }
}
