using InternIntelligence_UserLogin.Core.DTOs.Auth;
using System.Net.Http.Json;
using System.Net;
using InternIntelligence_UserLogin.API;
using FluentAssertions;
using InternIntelligence_UserLogin.Tests.Common.Factories;
using InternIntelligence_UserLogin.Core.DTOs.Token;
using Newtonsoft.Json.Linq;
using InternIntelligence_UserLogin.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using InternIntelligence_UserLogin.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using InternIntelligence_UserLogin.Infrastructure;

namespace InternIntelligence_UserLogin.Tests.Integration
{
    public class AuthEndpointsTests : IClassFixture<TestingWebAppFactory<Program>>
    {
        private readonly TestingWebAppFactory<Program> _factory;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly HttpClient _client;
        private readonly AppDbContext _context;

        public AuthEndpointsTests(TestingWebAppFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _userManager = factory.Services.CreateScope().ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            _context = factory.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
        }

        [Fact]
        public async Task Register_WhenValidRequest_ShouldReturnSuccessStatusCodeAndUserId()
        {
            // Arrange
            var registerDto = Factory.Auth.GenerateValidRegisterRequest();

            // Act
            var response = await _client.PostAsJsonAsync("api/auth/register", registerDto);

            // Assert
            var userId = await response.Content.ReadFromJsonAsync<Guid>();

            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            userId.Should().NotBeEmpty();

            //Cleanup
            await CleanupDataAsync();
        }

        [Fact]
        public async Task Register_WhenInValidRequest_ShouldReturnFailureStatusCode()
        {
            // Arrange
            var registerDto = Factory.Auth.GenerateInValidRegisterRequest();

            // Act
            var response = await _client.PostAsJsonAsync("api/auth/register", registerDto);

            // Assert
            response.StatusCode.Should().NotBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Register_WhenConfirmPasswordInValidRequest_ShouldReturnFailureStatusCode()
        {
            // Arrange
            var registerDto = Factory.Auth.GeneratePasswordsInValidRegisterRequest();

            // Act
            var response = await _client.PostAsJsonAsync("api/auth/register", registerDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            //Cleanup
            await CleanupDataAsync();
        }

        [Fact]
        public async Task Login_WhenValidCredentials_ShouldReturnToken()
        {
            // Arrange
            var registerDto = Factory.Auth.GenerateValidRegisterRequest();
            var registerResponse = await _client.PostAsJsonAsync("api/auth/register", registerDto);

            registerResponse.EnsureSuccessStatusCode();

            // Manually confirm email
            _factory.ManuallyConfirmEmail(registerDto.Email);

            var loginDto = Factory.Auth.GenerateValidLoginRequest();

            // Act
            var response = await _client.PostAsJsonAsync("api/auth/login", loginDto);

            // Assert
            response.EnsureSuccessStatusCode();
            var token = await response.Content.ReadFromJsonAsync<TokenDTO>();

            token.Should().NotBeNull();
            token.AccessToken.Should().NotBeNullOrEmpty();

            //Cleanup
            await CleanupDataAsync();
        }

        [Fact]
        public async Task Login_WhenInvalidCredentials_ShouldReturnBadRequest()
        {
            // Arrange
            var registerDto = Factory.Auth.GenerateValidRegisterRequest();
            var registerResponse = await _client.PostAsJsonAsync("api/auth/register", registerDto);

            registerResponse.EnsureSuccessStatusCode();

            // Manually confirm email
            _factory.ManuallyConfirmEmail(registerDto.Email);
            var loginDto = Factory.Auth.GenerateInValidLoginRequest();

            // Act
            var response = await _client.PostAsJsonAsync("api/auth/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            //Cleanup
            await CleanupDataAsync();
        }

        // ✅ Refresh Login Tests
        [Fact]
        public async Task RefreshLogin_WhenValidRequest_ShouldReturnNewToken()
        {
            // Arrange
            var registerDto = Factory.Auth.GenerateValidRegisterRequest();
            var registerResponse = await _client.PostAsJsonAsync("api/auth/register", registerDto);

            registerResponse.EnsureSuccessStatusCode();

            // Manually confirm email
            _factory.ManuallyConfirmEmail(registerDto.Email);

            var loginDto = Factory.Auth.GenerateValidLoginRequest();

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

            //Cleanup
            await CleanupDataAsync();
        }

        [Fact]
        public async Task RefreshLogin_WhenInvalidToken_ShouldNotReturnOk()
        {
            // Arrange
            var refreshDto = new RefreshLoginDTO
            {
                AccessToken = "wrong-access",
                RefreshToken = "wrong-refresh"
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/auth/refresh-login", refreshDto);

            // Assert
            response.StatusCode.Should().NotBe(HttpStatusCode.OK);
        }

        // ✅ Confirm Email Tests
        [Fact]
        public async Task ConfirmEmail_WhenValidRequest_ShouldReturnSuccess()
        {
            // Arrange
            var registerDto = Factory.Auth.GenerateValidRegisterRequest();
            var registerResponse = await _client.PostAsJsonAsync("api/auth/register", registerDto);

            registerResponse.EnsureSuccessStatusCode();
            var userId = await registerResponse.Content.ReadFromJsonAsync<Guid>();

            // Fetch the user from the database or from the registration response
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            // Get email confirmation token using UserManager
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = token.UrlEncode();

            // Act
            var response = await _client.PatchAsync($"api/auth/confirm-email?userId={userId}&token={encodedToken}", null);

            // Assert
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            //Cleanup
            await CleanupDataAsync();
        }

        [Fact]
        public async Task ConfirmEmail_WhenInvalidToken_ShouldReturnBadRequest()
        {
            // Arrange
            var registerDto = Factory.Auth.GenerateValidRegisterRequest();
            var registerResponse = await _client.PostAsJsonAsync("api/auth/register", registerDto);

            registerResponse.EnsureSuccessStatusCode();
            var userId = await registerResponse.Content.ReadFromJsonAsync<Guid>();
            var encodedToken = "invalid".UrlEncode();


            // Act
            var response = await _client.PatchAsync($"api/auth/confirm-email?userId={userId}&token={encodedToken}", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            //Cleanup
            await CleanupDataAsync();
        }


        private async Task CleanupDataAsync()
        {
            var users = _context.Users.ToList();
            _context.Users.RemoveRange(users);

            // concurrency error after updating email
            await _context.SaveChangesAsync();
        }
    }
}
