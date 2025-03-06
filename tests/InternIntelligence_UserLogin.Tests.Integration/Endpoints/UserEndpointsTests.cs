using InternIntelligence_UserLogin.API;
using InternIntelligence_UserLogin.Core.DTOs.Role;
using InternIntelligence_UserLogin.Core.DTOs.User;
using InternIntelligence_UserLogin.Core.Entities;
using InternIntelligence_UserLogin.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Net;
using FluentAssertions;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using InternIntelligence_UserLogin.Infrastructure;
using InternIntelligence_UserLogin.Tests.Common.Factories;

namespace InternIntelligence_UserLogin.Tests.Integration.Endpoints
{
    [Collection("Sequential")]
    public class UserEndpointsTests : IClassFixture<TestingWebApplicationFactory>, IAsyncLifetime
    {
        private readonly TestingWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly IServiceScope _scope;
        private readonly AppDbContext _context;

        public UserEndpointsTests(TestingWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _scope = _factory.Services.CreateScope();
            _context = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        }

        public async Task InitializeAsync()
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
        }

        public async Task DisposeAsync()
        {
            await _context.Database.EnsureDeletedAsync();

            _scope.Dispose();
            _context.Dispose();
            _client.Dispose();
        }

        [Fact]
        public async Task GetAllUsers_WhenWithAdminToken_ShouldReturnAllUsers()
        {
            // Arrange
            var accessToken = await _client.GetSuperAdminTokenAsync(_scope);

            int usersCount = 2;
            int expectedUsersCount = usersCount + 1; // extra admin

            await _client.RegisterUsersAsync(usersCount);

            var request = new HttpRequestMessage(HttpMethod.Get, "api/users");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var users = await response.Content.ReadFromJsonAsync<List<UserDTO>>();
            users.Should().NotBeNull();
            users.Count.Should().Be(expectedUsersCount);
        }

        [Fact]
        public async Task GetUserById_WhenWithAdminToken_ShouldReturnUser()
        {
            // Arrange
            var userId = await _client.RegisterSingleUserAsync();

            // Act
            var response = await _client.SendRequestWithAccessToken(HttpMethod.Get, $"api/users/{userId}", _scope);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var retrievedUser = await response.Content.ReadFromJsonAsync<UserDTO>();
            retrievedUser.Should().NotBeNull();
            retrievedUser.Id.Should().Be(userId);
        }

        [Fact]
        public async Task GetUserById_WhenWithUserToken_ShouldReturnUser()
        {
            // Arrange
            var (accessToken, userId) = await _client.RegisterAndLoginSingleUserAsync();

            // Act
            var response = await _client.SendRequestWithAccessToken(HttpMethod.Get, $"api/users/{userId}", _scope, accessToken: accessToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var retrievedUser = await response.Content.ReadFromJsonAsync<UserDTO>();
            retrievedUser.Should().NotBeNull();
            retrievedUser.Id.Should().Be(userId);
        }

        [Fact]
        public async Task GetUserById_WhenWithoutUserOrAdminToken_ShouldReturnUnauthorized()
        {
            // Arrange
            var userId = await _client.RegisterSingleUserAsync();

            // Act
            var response = await _client.GetAsync($"api/users/{userId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetUserRoles_WhenWithAdminToken_ShouldReturnUserRoles()
        {
            // Arrange
            var accessToken = await _client.GetSuperAdminTokenAsync(_scope);

            var userId = await _client.RegisterSingleUserAsync();
            var roleId = await _client.CreateRoleAsync(accessToken);

            await _client.AssignRoleToUserAsync(userId, roleId, accessToken);

            // Act
            var response = await _client.SendRequestWithAccessToken(HttpMethod.Get, $"api/users/{userId}/roles", _scope, accessToken: accessToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var roles = await response.Content.ReadFromJsonAsync<List<RoleDTO>>();
            roles.Should().NotBeNull();
            roles.Should().ContainSingle();
            roles.First().Id.Should().Be(roleId);
        }

        [Fact]
        public async Task AssignRolesToUser_WhenWithAdminToken_ShouldAssignRolesSuccessfully()
        {
            // Arrange
            var accessToken = await _client.GetSuperAdminTokenAsync(_scope);

            var userId = await _client.RegisterSingleUserAsync();
            var roleId = await _client.CreateRoleAsync(accessToken);

            // Act
            var requestBody = new AssignRolesDTO
            {
                RoleIds = [roleId]
            };

            var response = await _client.SendRequestWithAccessToken(HttpMethod.Patch, $"api/users/{userId}/assign-roles", _scope, requestBody, accessToken: accessToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify Role Assignment
            var assignedRolesResponse = await _client.SendRequestWithAccessToken(HttpMethod.Get, $"api/users/{userId}/roles", _scope, accessToken: accessToken);

            var roles = await assignedRolesResponse.Content.ReadFromJsonAsync<List<RoleDTO>>();

            roles.Should().ContainSingle(r => r.Id == roleId);
        }

        [Fact]
        public async Task RequestPasswordReset_WhenWithUserToken_ShouldReturnOk()
        {
            // Arrange
            var (accessToken, userId) = await _client.RegisterAndLoginSingleUserAsync();

            // Act
            var response = await _client.SendRequestWithAccessToken(HttpMethod.Get, $"api/users/{userId}/reset-password", _scope, accessToken: accessToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ResetPassword_WhenWithUserToken_ShouldResetUserPassword()
        {
            // Arrange
            var (accessToken, userId) = await _client.RegisterAndLoginSingleUserAsync();

            var resetTokenResponse = await _client.SendRequestWithAccessToken(HttpMethod.Get, $"api/users/{userId}/reset-password", _scope, accessToken: accessToken);

            resetTokenResponse.EnsureSuccessStatusCode();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new InvalidOperationException("User is not found.");
            }

            var _userManager = _scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = token.UrlEncode();

            var newPassword = "NewStrongPassword123!";

            var requestBody = new ResetPasswordDTO
            {
                NewPassword = newPassword
            };

            // Act
            var response = await _client.SendRequestWithAccessToken(HttpMethod.Patch, $"api/users/reset-password?userId={userId}&token={encodedToken}", _scope, requestBody, accessToken: accessToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAllUsers_WhenWithoutToken_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("api/users");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetUserById_WhenInvalidTokenProvided_ShouldReturnUnauthorized()
        {
            // Arrange
            var userId = await _client.RegisterSingleUserAsync();
            var invalidToken = Factories.Auth.GenerateInValidAccessToken();

            // Act
            var response = await _client.SendRequestWithAccessToken(HttpMethod.Get, $"api/users/{userId}", _scope, accessToken: invalidToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        }

        [Fact]
        public async Task AssignRolesToUser_WhenWithUserToken_ShouldReturnForbidden()
        {
            // Arrange
            var accessToken = await _client.GetSuperAdminTokenAsync(_scope);

            var (userToken, userId) = await _client.RegisterAndLoginSingleUserAsync();

            var roleId = await _client.CreateRoleAsync(accessToken);

            var requestBody = new AssignRolesDTO
            {
                RoleIds = [roleId]
            };

            // Act
            var response = await _client.SendRequestWithAccessToken(HttpMethod.Patch, $"api/users/{userId}/assign-roles", _scope, requestBody, accessToken: userToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task AssignRolesToUser_WhenWithInvalidRoleId_ShouldReturnNotFound()
        {
            // Arrange
            var accessToken = await _client.GetSuperAdminTokenAsync(_scope);

            var userId = await _client.RegisterSingleUserAsync();
            var invalidRoleId = Guid.NewGuid();

            var requestBody = new AssignRolesDTO
            {
                RoleIds = [invalidRoleId]
            };

            // Act
            var response = await _client.SendRequestWithAccessToken(HttpMethod.Patch, $"api/users/{userId}/assign-roles", _scope, requestBody, accessToken: accessToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }


        // NEW POLICY NEEDED (SHOULD TAKE USER ID FROM QUERY)
        [Fact]
        public async Task ResetPassword_WhenWithInvalidToken_ShouldReturnBadRequest()
        {
            // Arrange
            var (accessToken, userId) = await _client.RegisterAndLoginSingleUserAsync();

            var invalidToken = "invalid-reset-token".UrlEncode();
            var newPassword = "NewStrongPassword123!";

            var requestBody = new ResetPasswordDTO
            {
                NewPassword = newPassword
            };

            // Act
            var response = await _client.SendRequestWithAccessToken(HttpMethod.Patch, $"api/users/reset-password?userId={userId}&token={invalidToken}", _scope, requestBody, accessToken: accessToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetUserById_WhenWithInvalidUserId_ShouldReturnNotFound()
        {
            // Arrange
            var accessToken = await _client.GetSuperAdminTokenAsync(_scope);

            var invalidUserId = Guid.NewGuid();

            // Act
            var response = await _client.SendRequestWithAccessToken(HttpMethod.Get, $"api/users/{invalidUserId}", _scope, accessToken: accessToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
