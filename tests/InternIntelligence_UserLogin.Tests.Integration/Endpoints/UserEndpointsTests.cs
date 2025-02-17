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

namespace InternIntelligence_UserLogin.Tests.Integration.Endpoints
{
    [Collection("Sequential")]
    public class UserEndpointsTests : IClassFixture<TestingWebAppFactory>, IAsyncLifetime
    {
        private readonly TestingWebAppFactory _factory;
        private readonly HttpClient _client;
        private readonly IServiceScope _scope;
        private readonly AppDbContext _context;

        public UserEndpointsTests(TestingWebAppFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _scope = factory.Services.CreateScope();
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
            _client.Dispose();
        }

        [Fact]
        public async Task GetAllUsers_WhenWithAdminToken_ShouldReturnAllUsers()
        {
            // Arrange
            var _superAdminToken = await _client.GetSuperAdminTokenAsync(_scope);

            int usersCount = 2;
            int expectedUsersCount = usersCount + 1; // extra admin

            await _client.RegisterUsers(usersCount);

            var request = new HttpRequestMessage(HttpMethod.Get, "api/users");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _superAdminToken);

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
            var _superAdminToken = await _client.GetSuperAdminTokenAsync(_scope);

            var userId = await _client.RegisterSingleUser();

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/users/{userId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _superAdminToken);

            // Act
            var response = await _client.SendAsync(request);

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
            var (accessToken, userId) = await _client.RegisterAndLoginSingleUser(_scope);

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/users/{userId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Act
            var response = await _client.SendAsync(request);

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
            var userId = await _client.RegisterSingleUser();

            // Act
            var response = await _client.GetAsync($"api/users/{userId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetUserRoles_WhenWithAdminToken_ShouldReturnUserRoles()
        {
            // Arrange
            var _superAdminToken = await _client.GetSuperAdminTokenAsync(_scope);

            var userId = await _client.RegisterSingleUser();
            var roleId = await _client.CreateRole(_superAdminToken);

            await _client.AssignRoleToUser(userId, roleId, _superAdminToken);

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/users/{userId}/roles");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _superAdminToken);

            // Act
            var response = await _client.SendAsync(request);

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
            var _superAdminToken = await _client.GetSuperAdminTokenAsync(_scope);

            var userId = await _client.RegisterSingleUser();
            var roleId = await _client.CreateRole(_superAdminToken);

            var request = new HttpRequestMessage(HttpMethod.Patch, $"api/users/{userId}/assign-roles")
            {
                Content = JsonContent.Create(new AssignRolesDTO
                {
                    RoleIds = [roleId]
                })
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _superAdminToken);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify Role Assignment
            var assignedRolesRequest = new HttpRequestMessage(HttpMethod.Get, $"api/users/{userId}/roles");

            assignedRolesRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _superAdminToken);

            var assignedRolesResponse = await _client.SendAsync(assignedRolesRequest);
            var roles = await assignedRolesResponse.Content.ReadFromJsonAsync<List<RoleDTO>>();

            roles.Should().ContainSingle(r => r.Id == roleId);
        }

        [Fact]
        public async Task RequestPasswordReset_WhenWithUserToken_ShouldReturnOk()
        {
            // Arrange
            var (accessToken, userId) = await _client.RegisterAndLoginSingleUser(_scope);

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/users/{userId}/reset-password");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ResetPassword__WhenWithUserToken_ShouldResetUserPassword()
        {
            // Arrange
            var (accessToken, userId) = await _client.RegisterAndLoginSingleUser(_scope);

            var resetTokenRequest = new HttpRequestMessage(HttpMethod.Get, $"api/users/{userId}/reset-password");

            resetTokenRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var resetTokenResponse = await _client.SendAsync(resetTokenRequest);

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

            var request = new HttpRequestMessage(HttpMethod.Patch, $"api/users/reset-password?userId={userId}&token={encodedToken}")
            {
                Content = JsonContent.Create(new ResetPasswordDTO
                {
                    NewPassword = newPassword
                })
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Act
            var response = await _client.SendAsync(request);

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
            var userId = await _client.RegisterSingleUser();

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/users/{userId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        }

        [Fact]
        public async Task AssignRolesToUser_WhenWithUserToken_ShouldReturnForbidden()
        {
            // Arrange
            var _superAdminToken = await _client.GetSuperAdminTokenAsync(_scope);

            var (userToken, userId) = await _client.RegisterAndLoginSingleUser(_scope);
            var roleId = await _client.CreateRole(_superAdminToken);

            var request = new HttpRequestMessage(HttpMethod.Patch, $"api/users/{userId}/assign-roles")
            {
                Content = JsonContent.Create(new List<Guid> { roleId })
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task AssignRolesToUser_WhenWithInvalidRoleId_ShouldReturnNotFound()
        {
            // Arrange
            var _superAdminToken = await _client.GetSuperAdminTokenAsync(_scope);

            var userId = await _client.RegisterSingleUser();
            var invalidRoleId = Guid.NewGuid();

            var request = new HttpRequestMessage(HttpMethod.Patch, $"api/users/{userId}/assign-roles")
            {
                Content = JsonContent.Create(new AssignRolesDTO
                {
                    RoleIds = [invalidRoleId]
                })
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _superAdminToken);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }


        // NEW POLICY NEEDED (SHOULD TAKE USER ID FROM QUERY)
        [Fact]
        public async Task ResetPassword_WhenWithInvalidToken_ShouldReturnBadRequest()
        {
            // Arrange
            var (accessToken, userId) = await _client.RegisterAndLoginSingleUser(_scope);

            var invalidToken = "invalid-reset-token".UrlEncode();
            var newPassword = "NewStrongPassword123!";

            var request = new HttpRequestMessage(HttpMethod.Patch, $"api/users/reset-password?userId={userId}&token={invalidToken}")
            {
                Content = JsonContent.Create(newPassword)
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetUserById_WhenWithInvalidUserId_ShouldReturnNotFound()
        {
            // Arrange
            var _superAdminToken = await _client.GetSuperAdminTokenAsync(_scope);

            var invalidUserId = Guid.NewGuid();

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/users/{invalidUserId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _superAdminToken);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
