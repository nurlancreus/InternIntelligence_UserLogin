using InternIntelligence_UserLogin.Core.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace InternIntelligence_UserLogin.Endpoints
{
    public static class User
    {
        public static IEndpointRouteBuilder RegisterUserEndpoints(this IEndpointRouteBuilder routes)
        {
            var user = routes.MapGroup("api/user");

            user.MapGet("", async (IUserService userService, CancellationToken cancellationToken) =>
            {
                var users = await userService.GetAllAsync(cancellationToken);

                return Results.Ok(users);
            });

            user.MapGet("{id}", async ([FromRoute] string id, IUserService userService, CancellationToken cancellationToken) =>
            {
                var user = await userService.GetAsync(id, cancellationToken);

                return Results.Ok(user);
            });

            return routes;
        }
    }
}
