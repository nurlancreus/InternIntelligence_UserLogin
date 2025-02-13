using InternIntelligence_UserLogin.API;
using InternIntelligence_UserLogin.Core.DTOs.Auth;
using InternIntelligence_UserLogin.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternIntelligence_UserLogin.Tests.Integration
{
    public static class Extensions
    {
        public static TestingWebAppFactory<Program> ManuallyConfirmEmail(this TestingWebAppFactory<Program> factory, string email)
        {
            // Manually confirm email
            using var scope = factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var user = userManager.FindByEmailAsync(email).Result;

            user.EmailConfirmed = true;

            userManager.UpdateAsync(user).Wait();

            return factory;
        }
    }
}
