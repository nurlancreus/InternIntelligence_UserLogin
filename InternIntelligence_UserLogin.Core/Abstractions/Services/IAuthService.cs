using InternIntelligence_UserLogin.Core.DTOs.Auth;
using InternIntelligence_UserLogin.Core.DTOs.Token;

namespace InternIntelligence_UserLogin.Core.Abstractions.Services
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterDTO registerDTO);
        Task<TokenDTO> LoginAsync(LoginDTO loginDTO);
        Task<TokenDTO> RefreshLoginAsync(RefreshLoginDTO refreshLoginDTO);
        Task ConfirmEmailAsync(Guid userId, string token);
        Task RequestPasswordResetAsync(Guid userId);
        Task ResetPasswordAsync(Guid userId, string token, string newPassword);
    }
}
