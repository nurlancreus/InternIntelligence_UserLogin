using InternIntelligence_UserLogin.Core.DTOs.Role;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Net;
using FluentAssertions;
using System.Net.Http.Headers;
using InternIntelligence_UserLogin.Core.DTOs.User;
using InternIntelligence_UserLogin.Core.Entities;
using Microsoft.AspNetCore.Identity;
using InternIntelligence_UserLogin.Infrastructure.Persistence.Context;

namespace InternIntelligence_UserLogin.Tests.Integration.Endpoints
{
    [Collection("Sequential")]
    public class RoleEndpointsTests : IClassFixture<TestingWebAppFactory>, IAsyncLifetime
    {
        private readonly TestingWebAppFactory _factory;
        private readonly HttpClient _client;
        private readonly IServiceScope _scope;
        private readonly AppDbContext _context;

        public RoleEndpointsTests(TestingWebAppFactory factory)
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
            _client.Dispose();
        }

        [Fact]
        public async Task GetAllRoles_WhenWithSuperAdminToken_ShouldReturnAllRoles()
        {
            // Arrange
            var _superAdminToken = await _client.GetSuperAdminTokenAsync(_scope);

            int rolesCount = 2;
            int expectedRolesCount = rolesCount + 1; // extra admin

            await _client.CreateRoles(_superAdminToken, rolesCount);

            var request = new HttpRequestMessage(HttpMethod.Get, "api/roles");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _superAdminToken);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var roles = await response.Content.ReadFromJsonAsync<List<RoleDTO>>();
            roles.Should().NotBeNull();
            roles.Should().ContainItemsAssignableTo<RoleDTO>();
            roles.Count.Should().Be(expectedRolesCount);
        }

        [Fact]
        public async Task GetRoleById_WhenWithSuperAdminToken_ShouldReturnRole()
        {
            // Arrange
            var _superAdminToken = await _client.GetSuperAdminTokenAsync(_scope);

            var roleId = await _client.CreateRole(_superAdminToken);

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/roles/{roleId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _superAdminToken);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var role = await response.Content.ReadFromJsonAsync<RoleDTO>();
            role.Should().NotBeNull();
            role.Id.Should().Be(roleId);
        }

        [Fact]
        public async Task GetRoleUsers_WhenWithSuperAdminToken_ShouldReturnUsersInRole()
        {
            // Arrange
            var _superAdminToken = await _client.GetSuperAdminTokenAsync(_scope);

            var roleId = await _client.CreateRole(_superAdminToken);
            var userId = await _client.RegisterSingleUser();
            await _client.AssignRoleToUser(userId, roleId, _superAdminToken);

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/roles/{roleId}/users");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _superAdminToken);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var users = await response.Content.ReadFromJsonAsync<List<UserDTO>>();
            users.Should().NotBeNull();
            users.Should().ContainSingle(u => u.Id == userId);
        }

        [Fact]
        public async Task CreateRole_WhenWithSuperAdminTokenAndValidRequest_ShouldCreateRoleSuccessfully()
        {
            // Arrange
            var _superAdminToken = await _client.GetSuperAdminTokenAsync(_scope);

            var createRoleDto = new CreateRoleDTO
            {
                Name = "NewRole"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "api/roles")
            {
                Content = JsonContent.Create(createRoleDto)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _superAdminToken);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var createdRoleId = await response.Content.ReadFromJsonAsync<Guid>();
            createdRoleId.Should().NotBeEmpty();
        }

        [Fact]
        public async Task UpdateRole_WhenWithSuperAdminTokenAndValidRequest_ShouldUpdateRoleSuccessfully()
        {
            // Arrange
            var _superAdminToken = await _client.GetSuperAdminTokenAsync(_scope);

            var roleId = await _client.CreateRole(_superAdminToken);
            var updateRoleDto = new UpdateRoleDTO
            {
                Name = "UpdatedRole"
            };

            var request = new HttpRequestMessage(HttpMethod.Patch, $"api/roles/{roleId}")
            {
                Content = JsonContent.Create(updateRoleDto)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _superAdminToken);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updatedRoleId = await response.Content.ReadFromJsonAsync<Guid>();
            updatedRoleId.Should().Be(roleId);
        }

        [Fact]
        public async Task DeleteRole_WhenWithSuperAdminToken_ShouldDeleteRoleSuccessfully()
        {
            // Arrange
            var _superAdminToken = await _client.GetSuperAdminTokenAsync(_scope);

            var roleId = await _client.CreateRole(_superAdminToken);

            var request = new HttpRequestMessage(HttpMethod.Delete, $"api/roles/{roleId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _superAdminToken);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify deletion
            var deletedRole = await _context.Roles.FindAsync(roleId);
            deletedRole.Should().BeNull();
        }

        [Fact]
        public async Task AssignUsersToRole_WhenWithSuperAdminToken_ShouldAssignUsersSuccessfully()
        {
            // Arrange
            var _superAdminToken = await _client.GetSuperAdminTokenAsync(_scope);

            var roleId = await _client.CreateRole(_superAdminToken);
            var userId = await _client.RegisterSingleUser();

            var userManager = _scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var user = await userManager.FindByIdAsync(userId.ToString());

            var assignUsersDto = new List<string> { user!.UserName! };

            var request = new HttpRequestMessage(HttpMethod.Patch, $"api/roles/{roleId}/assign-users")
            {
                Content = JsonContent.Create(assignUsersDto)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _superAdminToken);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify user role assignment
            var assignedRolesRequest = new HttpRequestMessage(HttpMethod.Get, $"api/users/{userId}/roles");

            assignedRolesRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _superAdminToken);

            var assignedRolesResponse = await _client.SendAsync(assignedRolesRequest);
            var roles = await assignedRolesResponse.Content.ReadFromJsonAsync<List<RoleDTO>>();

            roles.Should().ContainSingle(r => r.Id == roleId);
        }

        [Fact]
        public async Task GetAllRoles_WhenWithoutToken_ShouldReturnUnauthorized()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "api/roles");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateRole_WhenWithoutToken_ShouldReturnUnauthorized()
        {
            // Arrange
            var createRoleDto = new CreateRoleDTO { Name = "UnauthorizedRole" };

            var request = new HttpRequestMessage(HttpMethod.Post, "api/roles")
            {
                Content = JsonContent.Create(createRoleDto)
            };

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateRole_WhenWithoutToken_ShouldReturnUnauthorized()
        {
            // Arrange
            var _superAdminToken = await _client.GetSuperAdminTokenAsync(_scope);

            var roleId = await _client.CreateRole(_superAdminToken);
            var updateRoleDto = new UpdateRoleDTO { Name = "UpdatedRole" };

            var request = new HttpRequestMessage(HttpMethod.Patch, $"api/roles/{roleId}")
            {
                Content = JsonContent.Create(updateRoleDto)
            };

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateRole_WhenWithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var _superAdminToken = await _client.GetSuperAdminTokenAsync(_scope);

            var createRoleDto = new CreateRoleDTO { Name = "" };

            var request = new HttpRequestMessage(HttpMethod.Post, "api/roles")
            {
                Content = JsonContent.Create(createRoleDto)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _superAdminToken);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateRole_WhenWithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var _superAdminToken = await _client.GetSuperAdminTokenAsync(_scope);

            var roleId = await _client.CreateRole(_superAdminToken);
            var updateRoleDto = new UpdateRoleDTO { Name = "" }; // Empty name

            var request = new HttpRequestMessage(HttpMethod.Patch, $"api/roles/{roleId}")
            {
                Content = JsonContent.Create(updateRoleDto)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _superAdminToken);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateRole_WhenWithNonExistentRoleId_ShouldReturnNotFound()
        {
            // Arrange
            var _superAdminToken = await _client.GetSuperAdminTokenAsync(_scope);

            var updateRoleDto = new UpdateRoleDTO { Name = "UpdatedRole" };
            var nonExistentRoleId = Guid.NewGuid();

            var request = new HttpRequestMessage(HttpMethod.Patch, $"api/roles/{nonExistentRoleId}")
            {
                Content = JsonContent.Create(updateRoleDto)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _superAdminToken);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
