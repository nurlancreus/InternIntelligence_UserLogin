using InternIntelligence_UserLogin.Core.DTOs.Role;
using InternIntelligence_UserLogin.Core.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternIntelligence_UserLogin.Core.Abstractions.Services
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDTO>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<RoleDTO> GetAsync(Guid roleId, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserDTO>> GetRoleUsers(Guid roleId, CancellationToken cancellationToken = default);
        Task<Guid> CreateAsync(CreateRoleDTO createRoleDTO, CancellationToken cancellationToken = default);
        Task<Guid> UpdateAsync(Guid roleId, UpdateRoleDTO updateRoleDTO, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid roleId, CancellationToken cancellationToken = default);
        Task AssignUsersToRoleAsync(Guid roleId, IEnumerable<string> userNames, CancellationToken cancellationToken = default);

    }
}
