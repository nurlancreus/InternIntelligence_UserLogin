using InternIntelligence_UserLogin.Core.DTOs.Auth;
using InternIntelligence_UserLogin.Core.DTOs.Token;

namespace InternIntelligence_UserLogin.Core.Abstractions
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterDTO registerDTO);
        Task<TokenDTO> LoginAsync(LoginDTO loginDTO);
        Task<TokenDTO> RefreshLoginAsync(RefreshLoginDTO refreshLoginDTO);
        Task ConfirmEmailAsync(string userId, string token);
        Task RequestPasswordResetAsync(string userId);
        Task ResetPasswordAsync(string userId, string token, string newPassword);
    }
}
