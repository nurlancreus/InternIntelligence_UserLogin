﻿using InternIntelligence_UserLogin.Core.DTOs.Role;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Net;
using FluentAssertions;
using System.Net.Http.Headers;
using InternIntelligence_UserLogin.Core.DTOs.User;
using InternIntelligence_UserLogin.Core.Entities;
using Microsoft.AspNetCore.Identity;
using InternIntelligence_UserLogin.Infrastructure.Persistence.Context;
using InternIntelligence_UserLogin.Tests.Common.Factories;

namespace InternIntelligence_UserLogin.Tests.Integration.Endpoints
{
    [Collection("Sequential")]
    public class RoleEndpointsTests : IClassFixture<TestingWebApplicationFactory>, IAsyncLifetime
    {
        private readonly TestingWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly IServiceScope _scope;
        private readonly AppDbContext _context;

        public RoleEndpointsTests(TestingWebApplicationFactory factory)
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
        public async Task GetAllRoles_WhenWithSuperAdminToken_ShouldReturnAllRoles()
        {
            // Arrange
            var accessToken = await _client.GetSuperAdminTokenAsync(_scope);

            int rolesCount = 2;
            int expectedRolesCount = rolesCount + 1; // extra admin

            await _client.CreateRolesAsync(accessToken, rolesCount);

            // Act
            var response = await _client.SendRequestWithAccessToken(HttpMethod.Get, "api/roles", _scope, accessToken: accessToken);

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
            var accessToken = await _client.GetSuperAdminTokenAsync(_scope);

            var roleId = await _client.CreateRoleAsync(accessToken);

            // Act
            var response = await _client.SendRequestWithAccessToken(HttpMethod.Get, $"api/roles/{roleId}", _scope, accessToken: accessToken);

            // Assert
            response.EnsureSuccessStatusCode();
            var role = await response.Content.ReadFromJsonAsync<RoleDTO>();
            role.Should().NotBeNull();
            role.Id.Should().Be(roleId);
        }

        [Fact]
        public async Task GetRoleUsers_WhenWithSuperAdminToken_ShouldReturnUsersInRole()
        {
            // Arrange
            var accessToken = await _client.GetSuperAdminTokenAsync(_scope);

            var roleId = await _client.CreateRoleAsync(accessToken);
            var userId = await _client.RegisterSingleUserAsync();

            await _client.AssignRoleToUserAsync(userId, roleId, accessToken);

            // Act
            var response = await _client.SendRequestWithAccessToken(HttpMethod.Get, $"api/roles/{roleId}/users", _scope, accessToken: accessToken);

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
            var accessToken = await _client.GetSuperAdminTokenAsync(_scope);

            var requestBody = new CreateRoleDTO { Name = "NewRole" };

            // Act
            var response = await _client.SendRequestWithAccessToken(HttpMethod.Post, "api/roles", _scope, requestBody, accessToken: accessToken);

            // Assert
            response.EnsureSuccessStatusCode();
            var createdRoleId = await response.Content.ReadFromJsonAsync<Guid>();
            createdRoleId.Should().NotBeEmpty();
        }

        [Fact]
        public async Task UpdateRole_WhenWithSuperAdminTokenAndValidRequest_ShouldUpdateRoleSuccessfully()
        {
            // Arrange
            var accessToken = await _client.GetSuperAdminTokenAsync(_scope);

            var roleId = await _client.CreateRoleAsync(accessToken);

            var requestBody = new UpdateRoleDTO
            {
                Name = "UpdatedRole"
            };

            // Act
            var response = await _client.SendRequestWithAccessToken(HttpMethod.Patch, $"api/roles/{roleId}", _scope, requestBody, accessToken: accessToken);

            // Assert
            response.EnsureSuccessStatusCode();
            var updatedRoleId = await response.Content.ReadFromJsonAsync<Guid>();
            updatedRoleId.Should().Be(roleId);
        }

        [Fact]
        public async Task DeleteRole_WhenWithSuperAdminToken_ShouldDeleteRoleSuccessfully()
        {
            // Arrange
            var accessToken = await _client.GetSuperAdminTokenAsync(_scope);

            var roleId = await _client.CreateRoleAsync(accessToken);

            // Act
            var response = await _client.SendRequestWithAccessToken(HttpMethod.Delete, $"api/roles/{roleId}", _scope, accessToken: accessToken);

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
            var accessToken = await _client.GetSuperAdminTokenAsync(_scope);

            var roleId = await _client.CreateRoleAsync(accessToken);
            var userId = await _client.RegisterSingleUserAsync();

            var userManager = _scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var user = await userManager.FindByIdAsync(userId.ToString());

            var requestBody = new AssignUsersDTO
            {
                UserNames = [user!.UserName!]
            };
            // Act
            var response = await _client.SendRequestWithAccessToken(HttpMethod.Patch, $"api/roles/{roleId}/assign-users", _scope, requestBody, accessToken: accessToken);

            // Assert
            response.EnsureSuccessStatusCode();

            // Verify user role assignment
            var assignedRolesResponse = await _client.SendRequestWithAccessToken(HttpMethod.Get, $"api/users/{userId}/roles", _scope, accessToken: accessToken);

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
        public async Task UpdateRole_WhenWithInValidToken_ShouldReturnUnauthorized()
        {
            // Arrange
            var accessToken = await _client.GetSuperAdminTokenAsync(_scope);
            var invalidAccesToken = Factories.Auth.GenerateInValidAccessToken();

            var roleId = await _client.CreateRoleAsync(accessToken);

            var requestBody = new UpdateRoleDTO { Name = "UpdatedRole" };

            // Act
            var response = await _client.SendRequestWithAccessToken(HttpMethod.Patch, $"api/roles/{roleId}", _scope, requestBody, accessToken: invalidAccesToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateRole_WhenWithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var accessToken = await _client.GetSuperAdminTokenAsync(_scope);

            var requestBody = new CreateRoleDTO { Name = "" };

            // Act
            var response = await _client.SendRequestWithAccessToken(HttpMethod.Post, "api/roles", _scope, requestBody, accessToken: accessToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateRole_WhenWithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var accessToken = await _client.GetSuperAdminTokenAsync(_scope);

            var roleId = await _client.CreateRoleAsync(accessToken);
            var requestBody = new UpdateRoleDTO { Name = "" };

            // Act
            var response = await _client.SendRequestWithAccessToken(HttpMethod.Patch, $"api/roles/{roleId}", _scope, requestBody, accessToken: accessToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateRole_WhenWithNonExistentRoleId_ShouldReturnNotFound()
        {
            // Arrange
            var accessToken = await _client.GetSuperAdminTokenAsync(_scope);

            var requestBody = new UpdateRoleDTO { Name = "UpdatedRole" };
            var nonExistentRoleId = Guid.NewGuid();

            // Act
            var response = await _client.SendRequestWithAccessToken(HttpMethod.Patch, $"api/roles/{nonExistentRoleId}", _scope, requestBody, accessToken: accessToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
