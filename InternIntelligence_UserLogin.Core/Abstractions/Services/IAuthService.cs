using InternIntelligence_UserLogin.Core.DTOs.Auth;
using InternIntelligence_UserLogin.Core.DTOs.Token;

namespace InternIntelligence_UserLogin.Core.Abstractions.Services
{
    public interface IAuthService
    {
        Task<Guid> RegisterAsync(RegisterDTO registerDTO);
        Task<TokenDTO> LoginAsync(LoginDTO loginDTO);
        Task<TokenDTO> RefreshLoginAsync(RefreshLoginDTO refreshLoginDTO);
        Task ConfirmEmailAsync(Guid userId, string token);
    }
}
