using InternIntelligence_UserLogin.Core.Entities;
using InternIntelligence_UserLogin.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace InternIntelligence_UserLogin.Tests.Integration
{
    public static class Extensions
    {
        public async static Task<AppDbContext> CreateNewDbContextAsync(this IServiceScope scope)
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            return context;
        }

        public static async Task ManuallyConfirmEmailAsync(this IServiceScope scope, string email)
        {
            // Manually confirm email
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var user = await userManager.FindByEmailAsync(email);

            if (user is null) throw new InvalidOperationException("User is not found, so operation is failed.");

            user.EmailConfirmed = true;

            await userManager.UpdateAsync(user);
        }
    }
}
