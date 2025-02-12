using InternIntelligence_UserLogin.Core.Abstractions.Services;
using InternIntelligence_UserLogin.Core.DTOs.Token;
using InternIntelligence_UserLogin.Core.Entities;
using InternIntelligence_UserLogin.Core.Options.Token;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace InternIntelligence_UserLogin.Infrastructure.Services
{
    public class TokenService(IOptions<TokenSettings> options, UserManager<ApplicationUser> userManager) : ITokenService
    {
        private readonly AccessSettings _accessSettings = options.Value.Access;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        public async Task<(string accessToken, DateTime tokenEndDate)> GenerateAccessTokenAsync(ApplicationUser user)
        {
            var claims = new List<Claim>()
            {
                new(ClaimTypes.Name, user.UserName!), // Name claim (username)
                new(ClaimTypes.GivenName, user.FirstName!),
                new(ClaimTypes.Surname, user.LastName!),
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // Subject (user id)
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT unique ID (JTI)
                new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString()), // Issued at (Unix timestamp)
                new(ClaimTypes.NameIdentifier, user.Id.ToString()), // Unique name identifier of the user (id)
                new(ClaimTypes.Email, user.Email!), // Email of the user
            };

            var userRoles = await _userManager.GetRolesAsync(user);

            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return GenerateAccessToken(claims);
        }

        public (string accessToken, DateTime tokenEndDate) GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var tokenEndDate = DateTime.UtcNow.AddMinutes(_accessSettings.AccessTokenLifeTimeInMinutes);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_accessSettings.SecurityKey));

            // Create the encrypted credentials.
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Set the token's configurations.
            var securityToken = new JwtSecurityToken(
                audience: _accessSettings.Audience,
                issuer: _accessSettings.Issuer,
                expires: tokenEndDate,
                notBefore: DateTime.UtcNow,
                signingCredentials: signingCredentials,
                claims: claims
            );

            // Create an instance of the token handler class.
            var tokenHandler = new JwtSecurityTokenHandler();
            var accessToken = tokenHandler.WriteToken(securityToken);

            return (accessToken, tokenEndDate);
        }

        public string GenerateRefreshToken()
        {
            byte[] number = new byte[64];

            using RandomNumberGenerator random = RandomNumberGenerator.Create();
            random.GetBytes(number);

            return Convert.ToBase64String(number);
        }

        public ClaimsPrincipal GetPrincipalFromAccessToken(string? accessToken)
        {
            var tokenValidationParameters = new TokenValidationParameters()
            {
                ValidateAudience = true,
                ValidAudience = _accessSettings.Audience,
                ValidateIssuer = true,
                ValidIssuer = _accessSettings.Issuer,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_accessSettings.SecurityKey)),

                ValidateLifetime = false //should be false
            };

            JwtSecurityTokenHandler jwtSecurityTokenHandler = new();

            ClaimsPrincipal principal = jwtSecurityTokenHandler.ValidateToken(accessToken, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }

        public async Task<TokenDTO> GetTokenDataAsync(ApplicationUser user)
        {
            var (accessToken, tokenEndDate) = await GenerateAccessTokenAsync(user);

            var refreshToken = GenerateRefreshToken();

            return new TokenDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenEndDate = tokenEndDate,
            };
        }
    }
}
