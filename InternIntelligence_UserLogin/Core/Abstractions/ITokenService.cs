using InternIntelligence_UserLogin.Core.Data.Entities;
using InternIntelligence_UserLogin.Core.DTOs.Token;
using System.Security.Claims;

namespace InternIntelligence_UserLogin.Core.Abstractions
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
