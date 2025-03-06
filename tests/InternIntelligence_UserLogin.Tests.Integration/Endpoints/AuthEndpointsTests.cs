using InternIntelligence_UserLogin.Core.DTOs.Auth;
using System.Net.Http.Json;
using System.Net;
using FluentAssertions;
using InternIntelligence_UserLogin.Tests.Common.Factories;
using InternIntelligence_UserLogin.Core.DTOs.Token;
using InternIntelligence_UserLogin.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using InternIntelligence_UserLogin.Infrastructure;
using InternIntelligence_UserLogin.Infrastructure.Persistence.Context;

namespace InternIntelligence_UserLogin.Tests.Integration.Endpoints
{
    [Collection("Sequential")]
    public class AuthEndpointsTests : IClassFixture<TestingWebApplicationFactory>, IAsyncLifetime
    {
        private readonly TestingWebApplicationFactory _factory;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly HttpClient _client;
        private readonly IServiceScope _scope;
        private readonly AppDbContext _context;

        public AuthEndpointsTests(TestingWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _scope = _factory.Services.CreateScope();
            _context = _scope.ServiceProvider.GetRequiredService<AppDbContext>();

            _userManager = _scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
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
        public async Task Register_WhenWithValidRequest_ShouldReturnSuccessStatusCodeAndUserId()
        {
            // Arrange
            var registerDto = Factories.Auth.GenerateValidRegisterRequest();

            // Act
            var response = await _client.PostAsJsonAsync("api/auth/register", registerDto);

            // Assert
            var userId = await response.Content.ReadFromJsonAsync<Guid>();

            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            userId.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Register_WhenWithInValidRequest_ShouldReturnBadRequest()
        {
            // Arrange
            var registerDto = Factories.Auth.GenerateInValidRegisterRequest();

            // Act
            var response = await _client.PostAsJsonAsync("api/auth/register", registerDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_WhenWithConfirmPasswordInValidRequest_ShouldReturnBadRequest()
        {
            // Arrange
            var registerDto = Factories.Auth.GeneratePasswordsInValidRegisterRequest();

            // Act
            var response = await _client.PostAsJsonAsync("api/auth/register", registerDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_WhenWithValidCredentials_ShouldReturnToken()
        {
            // Arrange
            await _client.RegisterSingleUserAsync();

            // Manually confirm email
            // await _scope.ManuallyConfirmEmailAsync(registerDto.Email);

            var loginDto = Factories.Auth.GenerateValidLoginRequest();

            // Act
            var response = await _client.PostAsJsonAsync("api/auth/login", loginDto);

            // Assert
            response.EnsureSuccessStatusCode();
            var token = await response.Content.ReadFromJsonAsync<TokenDTO>();

            token.Should().NotBeNull();
            token.AccessToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_WhenWithInvalidCredentials_ShouldReturnBadRequest()
        {
            // Arrange
            await _client.RegisterSingleUserAsync();

            // Manually confirm email
            // await _scope.ManuallyConfirmEmailAsync(registerDto.Email);
            var loginDto = Factories.Auth.GenerateInValidLoginRequest();

            // Act
            var response = await _client.PostAsJsonAsync("api/auth/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task RefreshLogin_WhenWithValidRequest_ShouldReturnNewToken()
        {
            // Arrange
            await _client.RegisterSingleUserAsync();

            var loginDto = Factories.Auth.GenerateValidLoginRequest();

            var loginResponse = await _client.PostAsJsonAsync("api/auth/login", loginDto);

            loginResponse.EnsureSuccessStatusCode();
            var token = await loginResponse.Content.ReadFromJsonAsync<TokenDTO>();

            token.Should().NotBeNull();

            var refreshDto = new RefreshLoginDTO
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/auth/refresh-login", refreshDto);

            // Assert
            response.EnsureSuccessStatusCode();
            var newToken = await response.Content.ReadFromJsonAsync<TokenDTO>();

            newToken.Should().NotBeNull();
            newToken.AccessToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task RefreshLogin_WhenWithInvalidToken_ShouldNotReturnOk()
        {
            // Arrange
            var refreshDto = new RefreshLoginDTO
            {
                AccessToken = "invalid-access",
                RefreshToken = "invalid-refresh"
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/auth/refresh-login", refreshDto);

            // Assert
            response.StatusCode.Should().NotBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ConfirmEmail_WhenWithValidRequest_ShouldReturnSuccess()
        {
            // Arrange
            var userId = await _client.RegisterSingleUserAsync();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new InvalidOperationException("User is not found.");
            }

            // Get email confirmation token using UserManager
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = token.UrlEncode();

            // Act
            var response = await _client.PatchAsync($"api/auth/confirm-email?userId={userId}&token={encodedToken}", null);

            // Assert
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ConfirmEmail_WhenWithInvalidToken_ShouldReturnBadRequest()
        {
            // Arrange
            var userId = await _client.RegisterSingleUserAsync();
            var invalidToken = "invalid";

            var encodedInValidToken = invalidToken.UrlEncode();

            // Act
            var response = await _client.PatchAsync($"api/auth/confirm-email?userId={userId}&token={encodedInValidToken}", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
