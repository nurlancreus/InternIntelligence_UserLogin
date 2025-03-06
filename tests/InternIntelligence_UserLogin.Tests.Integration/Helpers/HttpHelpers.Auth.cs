using FluentAssertions;
using System.Net.Http.Json;
using System.Net;
using InternIntelligence_UserLogin.Tests.Common.Factories;
using InternIntelligence_UserLogin.Tests.Common.Constants;
using InternIntelligence_UserLogin.Core.DTOs.Token;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using InternIntelligence_UserLogin.Core.Entities;
using InternIntelligence_UserLogin.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using InternIntelligence_UserLogin.Core.DTOs.Auth;

namespace InternIntelligence_UserLogin.Tests.Integration
{
    public static partial class HttpHelpers
    {
        public static async Task<ICollection<Guid>> RegisterUsersAsync(this HttpClient client, int usersCount = 4)
        {
            var registersDto = Factories.Auth.GenerateValidRegisterRequests(usersCount);
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

        public static async Task<Guid> RegisterSingleUserAsync(this HttpClient client)
        {
            var registerDto = Factories.Auth.GenerateValidRegisterRequest();

            var response = await client.PostAsJsonAsync("api/auth/register", registerDto);

            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var userId = await response.Content.ReadFromJsonAsync<Guid>();
            userId.Should().NotBeEmpty();

            return userId;
        }

        public static async Task<(string accessToken, Guid userId)> RegisterAndLoginSingleUserAsync(this HttpClient client)
        {
            var registerDto = Factories.Auth.GenerateValidRegisterRequest();

            var registerResponse = await client.PostAsJsonAsync("api/auth/register", registerDto);

            registerResponse.EnsureSuccessStatusCode();

            var userId = await registerResponse.Content.ReadFromJsonAsync<Guid>();
            userId.Should().NotBeEmpty();

            var loginDto = Factories.Auth.GenerateValidLoginRequest();

            // Act
            var response = await client.PostAsJsonAsync("api/auth/login", loginDto);

            // Assert
            response.EnsureSuccessStatusCode();
            var token = await response.Content.ReadFromJsonAsync<TokenDTO>();

            token.Should().NotBeNull();
            token.AccessToken.Should().NotBeNullOrEmpty();

            return (token.AccessToken, userId);
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

            if (!await userManager.IsInRoleAsync(superAdmin, Constants.Role.SuperAdmin))
            {
                var addRoleResult = await userManager.AddToRoleAsync(superAdmin, Constants.Role.SuperAdmin);

                if (!addRoleResult.Succeeded)
                {
                    throw new Exception($"Failed to assign 'SuperAdmin' role: {Helpers.GetIdentityResultError(addRoleResult)}");
                }
            }

            var loginRequest = new LoginDTO
            {
                Email = superAdminEmail,
                Password = password
            };

            var loginResponse = await client.PostAsJsonAsync("api/auth/login", loginRequest);
            loginResponse.EnsureSuccessStatusCode();

            var tokenResponse = await loginResponse.Content.ReadFromJsonAsync<TokenDTO>();

            if (tokenResponse == null) throw new InvalidOperationException("Token response is null.");

            return tokenResponse.AccessToken;
        }

        public async static Task<HttpResponseMessage> SendRequestWithAccessToken(this HttpClient client, HttpMethod httpMethod, string requestUrl, IServiceScope scope, object? requestBody = null, string? accessToken = null)
        {
            if (string.IsNullOrEmpty(accessToken))
                accessToken = await client.GetSuperAdminTokenAsync(scope);

            var request = new HttpRequestMessage(httpMethod, requestUrl);

            if (requestBody != null)
            {
                var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return await client.SendAsync(request);
        }
    }
}
