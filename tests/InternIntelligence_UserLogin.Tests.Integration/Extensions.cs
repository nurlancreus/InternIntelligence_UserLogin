using FluentAssertions;
using InternIntelligence_UserLogin.API;
using InternIntelligence_UserLogin.Core.DTOs.Auth;
using InternIntelligence_UserLogin.Core.Entities;
using InternIntelligence_UserLogin.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using InternIntelligence_UserLogin.Tests.Common.Factories;
using InternIntelligence_UserLogin.Infrastructure;
using InternIntelligence_UserLogin.Core.DTOs.Token;
using InternIntelligence_UserLogin.Tests.Common.Constants;
using InternIntelligence_UserLogin.Core.DTOs.Role;
using InternIntelligence_UserLogin.API.Endpoints;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;

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

        public static async Task<string> GetSuperAdminTokenAsync(this HttpClient client, IServiceScope scope)
        {
            var superAdminEmail = Constants.Auth.SuperAdmin_Email;
            var password = Constants.Auth.SuperAdmin_Password;

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            if (!await roleManager.RoleExistsAsync(Constants.Role.SuperAdmin))
            {
                var createRoleResult = await roleManager.CreateAsync(ApplicationRole.Create(Constants.Role.SuperAdmin));

                if (!createRoleResult.Succeeded)
                {
                    throw new Exception($"Failed to create 'SuperAdmin' role: {Helpers.GetIdentityResultError(createRoleResult)}");
                }
            }

            var superAdmin = await userManager.FindByEmailAsync(superAdminEmail);
            if (superAdmin == null)
            {
                superAdmin = ApplicationUser.Create(Constants.Auth.SuperAdmin_FirstName, Constants.Auth.SuperAdmin_LastName, Constants.Auth.SuperAdmin_UserName, superAdminEmail);

                superAdmin.EmailConfirmed = true;

                var createUserResult = await userManager.CreateAsync(superAdmin, password);
                if (!createUserResult.Succeeded)
                {
                    throw new Exception($"Failed to create Super Admin: {Helpers.GetIdentityResultError(createUserResult)}");
                }
            }

            var addRoleResult = await userManager.AddToRoleAsync(superAdmin, Constants.Role.SuperAdmin);

            if (!addRoleResult.Succeeded)
            {
                throw new Exception($"Failed to assign 'SuperAdmin' role: {Helpers.GetIdentityResultError(addRoleResult)}");
            }

            var loginRequest = new
            {
                Email = superAdminEmail,
                Password = password
            };

            var loginResponse = await client.PostAsJsonAsync("api/auth/login", loginRequest);
            loginResponse.EnsureSuccessStatusCode();

            var tokenResponse = await loginResponse.Content.ReadFromJsonAsync<TokenDTO>();
            return tokenResponse!.AccessToken;
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

        public static async Task<ICollection<Guid>> RegisterUsers(this HttpClient client, int usersCount = 4)
        {
            var registersDto = Factory.Auth.GenerateValidRegisterRequests(usersCount);
            ICollection<HttpResponseMessage> responses = [];
            ICollection<Guid> userIds = [];

            foreach (var registerDto in registersDto)
            {
                var response = await client.PostAsJsonAsync("api/auth/register", registerDto);

                responses.Add(response);
            }

            foreach (var response in responses)
            {
                response.EnsureSuccessStatusCode();
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var userId = await response.Content.ReadFromJsonAsync<Guid>();
                userId.Should().NotBeEmpty();
                userIds.Add(userId);
            }

            return userIds;
        }

        public static async Task<Guid> RegisterSingleUser(this HttpClient client)
        {
            var registerDto = Factory.Auth.GenerateValidRegisterRequest();

            var response = await client.PostAsJsonAsync("api/auth/register", registerDto);

            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var userId = await response.Content.ReadFromJsonAsync<Guid>();
            userId.Should().NotBeEmpty();

            return userId;
        }

        public static async Task<ICollection<Guid>> CreateRoles(this HttpClient client, string superAdminAccessToken, int rolesCount = 4)
        {
            var roleDtos = Factory.Role.GenerateGuestRoleRequests(rolesCount);
            ICollection<Guid> roleIds = [];

            foreach (var roleDto in roleDtos)
            {
                var roleId = await client.CreateRole(superAdminAccessToken, roleDto.Name);

                roleIds.Add(roleId);
            }

            return roleIds;
        }

        public static async Task<Guid> CreateRole(this HttpClient client, string superAdminAccessToken, string roleName = "Guest")
        {
            var roleDto = new CreateRoleDTO { Name = roleName };
            var request = new HttpRequestMessage(HttpMethod.Post, "api/roles")
            {
                Content = JsonContent.Create(roleDto)
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", superAdminAccessToken);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var roleId = await response.Content.ReadFromJsonAsync<Guid>();
            return roleId!;
        }

        public static async Task AssignRoleToUser(this HttpClient client, Guid userId, Guid roleId, string superAdminAccessToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Patch, $"api/users/{userId}/assign-roles")
            {
                Content = JsonContent.Create(new List<Guid> { roleId })
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", superAdminAccessToken);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }


        public static async Task<(string accessToken, Guid userId)> RegisterAndLoginSingleUser(this HttpClient client, IServiceScope scope)
        {
            var registerDto = Factory.Auth.GenerateValidRegisterRequest();

            var registerResponse = await client.PostAsJsonAsync("api/auth/register", registerDto);

            registerResponse.EnsureSuccessStatusCode();
            registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var userId = await registerResponse.Content.ReadFromJsonAsync<Guid>();
            userId.Should().NotBeEmpty();

            // await scope.ManuallyConfirmEmailAsync(registerDto.Email);

            var loginDto = Factory.Auth.GenerateValidLoginRequest();

            // Act
            var response = await client.PostAsJsonAsync("api/auth/login", loginDto);

            // Assert
            response.EnsureSuccessStatusCode();
            var token = await response.Content.ReadFromJsonAsync<TokenDTO>();

            token.Should().NotBeNull();
            token.AccessToken.Should().NotBeNullOrEmpty();

            return (token.AccessToken, userId);
        }
    }
}
