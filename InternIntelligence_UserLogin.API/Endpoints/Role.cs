using InternIntelligence_UserLogin.API.Validators;
using InternIntelligence_UserLogin.Core.Abstractions.Services;
using InternIntelligence_UserLogin.Core.DTOs.Role;
using Microsoft.AspNetCore.Mvc;

namespace InternIntelligence_UserLogin.API.Endpoints
{
    public static class Role
    {
        public static IEndpointRouteBuilder RegisterRoleEndpoints(this IEndpointRouteBuilder routes)
        {
            var role = routes.MapGroup("api/roles");

            role.MapGet("", async (IRoleService roleService, CancellationToken cancellationToken) =>
            {
                var roles = await roleService.GetAllAsync(cancellationToken);

                return Results.Ok(roles);
            });

            role.MapGet("{id}", async ([FromRoute] Guid id, IRoleService roleService, CancellationToken cancellationToken) =>
            {
                var roles = await roleService.GetAsync(id, cancellationToken);

                return Results.Ok(roles);
            });

            role.MapGet("{id}/users", async ([FromRoute] Guid id, IRoleService roleService, CancellationToken cancellationToken) =>
            {
                var users = await roleService.GetRoleUsers(id, cancellationToken);

                return Results.Ok(users);
            });

            role.MapPost("", async (IRoleService roleService, [FromBody] CreateRoleDTO roleDto, CancellationToken cancellationToken) =>
            {
                var id = await roleService.CreateAsync(roleDto, cancellationToken);

                return Results.Ok(id);
            }).Validate<CreateRoleDTO>();

            role.MapPatch("{id}", async ([FromRoute] Guid id, IRoleService roleService, [FromBody] UpdateRoleDTO roleDto, CancellationToken cancellationToken) =>
            {
                var roleId = await roleService.UpdateAsync(id, roleDto, cancellationToken);

                return Results.Ok(roleId);
            }).Validate<UpdateRoleDTO>();

            role.MapDelete("{id}", async ([FromRoute] Guid id, IRoleService roleService, CancellationToken cancellationToken) =>
            {
                await roleService.DeleteAsync(id, cancellationToken);

                return Results.Ok();
            });

            role.MapPatch("{id}/assign-users", async ([FromRoute] Guid id, [FromBody] IEnumerable<string> userNames, IRoleService roleService, CancellationToken cancellationToken) =>
            {
                await roleService.AssignUsersToRoleAsync(id, userNames, cancellationToken);

                return Results.Ok();
            });

            return routes;
        }
    }
}
