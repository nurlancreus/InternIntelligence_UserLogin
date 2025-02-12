using InternIntelligence_UserLogin.Core.DTOs.Role;
using InternIntelligence_UserLogin.Core.DTOs.User;
using InternIntelligence_UserLogin.Core.Entities;

namespace InternIntelligence_UserLogin.Core.Abstractions.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserDTO>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<UserDTO> GetAsync(Guid id, CancellationToken cancellationToken = default);
        Task UpdateRefreshTokenAsync(ApplicationUser user, string refreshToken, DateTime accessTokenEndDate);
        Task<IEnumerable<RoleDTO>> GetUserRoles(Guid id, CancellationToken cancellationToken = default);
        Task AssignRolesToUserAsync(Guid userId, IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default);
    }
}
