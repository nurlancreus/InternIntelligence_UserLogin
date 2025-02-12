using InternIntelligence_UserLogin.Core.DTOs.Token;
using InternIntelligence_UserLogin.Core.Entities;
using System.Security.Claims;

namespace InternIntelligence_UserLogin.Core.Abstractions.Services
{
    public interface ITokenService
    {
        Task<(string accessToken, DateTime tokenEndDate)> GenerateAccessTokenAsync(ApplicationUser user);
        (string accessToken, DateTime tokenEndDate) GenerateAccessToken(IEnumerable<Claim> claims);
        string GenerateRefreshToken();
        Task<TokenDTO> GetTokenDataAsync(ApplicationUser user);
        ClaimsPrincipal GetPrincipalFromAccessToken(string? accessToken);
    }
}
