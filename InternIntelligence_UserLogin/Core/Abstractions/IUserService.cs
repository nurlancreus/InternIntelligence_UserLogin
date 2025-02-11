using InternIntelligence_UserLogin.Core.Data.Entities;
using InternIntelligence_UserLogin.Core.DTOs.User;

namespace InternIntelligence_UserLogin.Core.Abstractions
{
    public interface IUserService
    {
        Task<IEnumerable<UserDTO>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<UserDTO> GetAsync(string id, CancellationToken cancellationToken = default);
        Task UpdateRefreshTokenAsync(ApplicationUser user, string refreshToken, DateTime accessTokenEndDate);
    }
}
