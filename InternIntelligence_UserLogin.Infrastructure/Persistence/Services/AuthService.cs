using InternIntelligence_UserLogin.Core.Abstractions.Services;
using InternIntelligence_UserLogin.Core.Abstractions.Services.Mail;
using InternIntelligence_UserLogin.Core.Abstractions.Session;
using InternIntelligence_UserLogin.Core.DTOs.Auth;
using InternIntelligence_UserLogin.Core.DTOs.Token;
using InternIntelligence_UserLogin.Core.Entities;
using InternIntelligence_UserLogin.Core.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace InternIntelligence_UserLogin.Infrastructure.Persistence.Services
{
    public class AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IUserService userService,
        ITokenService tokenService, IUserEmailService userEmailService, IJwtSession jwtSession) : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly IUserService _userService = userService;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IUserEmailService _userEmailService = userEmailService;
        private readonly IJwtSession _jwtSession = jwtSession;

        public async Task<TokenDTO> LoginAsync(LoginDTO loginDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginDTO.Email) ?? throw new LoginException();

            if (!user.EmailConfirmed) throw new LoginException("Email is not confirmed. Please verify your email.");

            var result = await _signInManager.PasswordSignInAsync(user, loginDTO.Password, false, false);

            if (!result.Succeeded)
            {
                if (result.IsLockedOut) throw new LoginException("Too many failed attempts. Try again in 5 minutes.");
                if (result.IsNotAllowed) throw new LoginException("User is not allowed to sign in.");

                throw new LoginException();
            }

            var tokenData = await _tokenService.GetTokenDataAsync(user);

            await _userService.UpdateRefreshTokenAsync(user, tokenData.RefreshToken, tokenData.AccessTokenEndDate);

            return tokenData;
        }

        public async Task RegisterAsync(RegisterDTO registerDTO)
        {
            var userWithSameEmail = await _userManager.FindByEmailAsync(registerDTO.Email);
            if (userWithSameEmail is not null)
                throw new RegisterException($"User with email '{registerDTO.Email}' already exists.");

            var userWithSameName = await _userManager.FindByNameAsync(registerDTO.Username);
            if (userWithSameName is not null)
                throw new RegisterException($"Username '{registerDTO.Username}' is already taken.");

            var user = ApplicationUser.Create(registerDTO.FirstName, registerDTO.LastName, registerDTO.Username, registerDTO.Email);

            var result = await _userManager.CreateAsync(user, registerDTO.Password);
            if (!result.Succeeded)
            {
                throw new RegisterException($"Registration failed: {Helpers.GetIdentityResultError(result)}");
            }

            var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            await _userEmailService.SendAccountConfirmationEmailAsync(user.Id, user.UserName!, user.Email!, emailConfirmationToken);
        }

        public async Task ConfirmEmailAsync(Guid userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null) throw new RegisterException("User not found.");

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                throw new RegisterException($"Email confirmation failed: {Helpers.GetIdentityResultError(result)}");
            }

            await _userEmailService.SendWelcomeEmailAsync(user.UserName!, user.Email!);
        }

        public async Task<TokenDTO> RefreshLoginAsync(RefreshLoginDTO refreshLoginDTO)
        {
            if (string.IsNullOrWhiteSpace(refreshLoginDTO.AccessToken) || string.IsNullOrWhiteSpace(refreshLoginDTO.RefreshToken))
                throw new SecurityTokenException("Access token and refresh token must be provided.");

            var principal = _tokenService.GetPrincipalFromAccessToken(refreshLoginDTO.AccessToken);
            if (principal is null) throw new SecurityTokenException("Invalid token.");

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) throw new SecurityTokenException("Invalid token.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) throw new SecurityTokenException("User not found.");

            if (user.RefreshToken != refreshLoginDTO.RefreshToken)
                throw new SecurityTokenException("Invalid refresh token.");

            if (user.RefreshTokenEndDate <= DateTime.UtcNow)
                throw new SecurityTokenException("Refresh token has expired.");

            var (accessToken, tokenEndDate) = _tokenService.GenerateAccessToken(principal.Claims);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            await _userService.UpdateRefreshTokenAsync(user, newRefreshToken, tokenEndDate);
            await _userManager.UpdateAsync(user);

            return new TokenDTO
            {
                AccessToken = accessToken,
                AccessTokenEndDate = tokenEndDate,
                RefreshToken = newRefreshToken,
            };
        }

        public async Task RequestPasswordResetAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user is null) throw new NotFoundException("User not found.");

            _jwtSession.IsUserAuthorized(user.Id, true);

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            await _userEmailService.SendResetPasswordEmailAsync(userId, user.UserName!, user.Email!, resetToken);
        }

        public async Task ResetPasswordAsync(Guid userId, string token, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user is null) throw new NotFoundException("User not found.");

            _jwtSession.IsUserAuthorized(user.Id, true);

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, newPassword);
            if (!result.Succeeded)
            {
                throw new PasswordResetException($"Password reset failed: {Helpers.GetIdentityResultError(result)}");
            }
        }
    }
}
