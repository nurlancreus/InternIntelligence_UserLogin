using InternIntelligence_UserLogin.Core.Abstractions.Services;
using InternIntelligence_UserLogin.Core.Abstractions.Session;
using InternIntelligence_UserLogin.Core.DTOs.Role;
using InternIntelligence_UserLogin.Core.DTOs.User;
using InternIntelligence_UserLogin.Core.Entities;
using InternIntelligence_UserLogin.Core.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace InternIntelligence_UserLogin.Infrastructure.Persistence.Services
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtSession _jwtSession;

        public RoleService(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, IJwtSession jwtSession)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _jwtSession = jwtSession;
        }

        public async Task AssignUsersToRoleAsync(Guid roleId, IEnumerable<string> userNames, CancellationToken cancellationToken = default)
        {
            _jwtSession.IsUserSuperAdmin(true);

            var role = await _roleManager.FindByIdAsync(roleId.ToString());

            if (role is null) throw new NotFoundException("Role is not found.");

            foreach (var userName in userNames)
            {
                var user = await _userManager.FindByNameAsync(userName);
                if (user is not null && !await _userManager.IsInRoleAsync(user, role.Name!))
                {
                    var result = await _userManager.AddToRoleAsync(user, role.Name!);
                    if (!result.Succeeded)
                    {
                        throw new UpdateNotSucceededException($"Failed to add user {userName} to role {role.Name}");
                    }
                }
            }

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            foreach (var user in usersInRole)
            {
                if (!userNames.Contains(user.UserName))
                {
                    var result = await _userManager.RemoveFromRoleAsync(user, role.Name!);
                    if (!result.Succeeded)
                    {
                        throw new UpdateNotSucceededException($"Failed to remove user {user.UserName} from role {role.Name}");
                    }
                }
            }
        }

        public async Task<Guid> CreateAsync(CreateRoleDTO createRoleDTO, CancellationToken cancellationToken = default)
        {
            _jwtSession.IsUserSuperAdmin(true);

            var existingRole = await _roleManager.FindByNameAsync(createRoleDTO.Name);
            if (existingRole != null) throw new ValidationException($"Role with the name '{existingRole.Name}' already exists.");

            var role = ApplicationRole.Create(createRoleDTO.Name);
            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                throw new CreateNotSucceededException($"Role creation failed: {Helpers.GetIdentityResultError(result)}");
            }

            return role.Id;
        }

        public async Task DeleteAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            _jwtSession.IsUserSuperAdmin(true);

            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null) throw new NotFoundException("Role is not found.");

            var result = await _roleManager.DeleteAsync(role);

            if (!result.Succeeded)
            {
                throw new DeleteNotSucceededException($"Role deletion failed: {Helpers.GetIdentityResultError(result)}");
            }
        }

        public async Task<IEnumerable<RoleDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            _jwtSession.IsUserAdmin(true);

            var roles = await _roleManager.Roles.Select(r => new RoleDTO(r)).ToListAsync(cancellationToken);
            return roles;
        }

        public async Task<RoleDTO> GetAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            _jwtSession.IsUserAdmin(true);

            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role is null) throw new NotFoundException("Role is not found.");

            return new RoleDTO(role);
        }

        public async Task<IEnumerable<UserDTO>> GetRoleUsers(Guid roleId, CancellationToken cancellationToken = default)
        {
            _jwtSession.IsUserSuperAdmin(true);

            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role is null) throw new NotFoundException("Role is not found.");

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            return usersInRole.Select(user => new UserDTO(user));
        }

        public async Task<Guid> UpdateAsync(Guid roleId, UpdateRoleDTO updateRoleDTO, CancellationToken cancellationToken = default)
        {
            _jwtSession.IsUserSuperAdmin(true);

            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role is null) throw new NotFoundException("Role is not found.");

            if (!string.IsNullOrEmpty(updateRoleDTO.Name) && updateRoleDTO.Name != role.Name)
            {
                var existingRole = await _roleManager.FindByNameAsync(updateRoleDTO.Name);
                if (existingRole != null) throw new ValidationException($"Role with the name '{existingRole.Name}' already exists.");

                role.Name = updateRoleDTO.Name;
            }

            var result = await _roleManager.UpdateAsync(role);

            if (!result.Succeeded)
            {
                throw new UpdateNotSucceededException($"Role update failed: {Helpers.GetIdentityResultError(result)}");
            }

            return role.Id;
        }
    }
}
