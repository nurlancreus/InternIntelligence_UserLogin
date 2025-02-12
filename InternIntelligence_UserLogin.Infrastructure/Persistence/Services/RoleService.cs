using InternIntelligence_UserLogin.Core.Abstractions.Services;
using InternIntelligence_UserLogin.Core.Abstractions.Session;
using InternIntelligence_UserLogin.Core.DTOs.Role;
using InternIntelligence_UserLogin.Core.DTOs.User;
using InternIntelligence_UserLogin.Core.Entities;
using InternIntelligence_UserLogin.Core.Entities.Join;
using InternIntelligence_UserLogin.Core.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace InternIntelligence_UserLogin.Infrastructure.Persistence.Services
{
    public class RoleService(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, IJwtSession jwtSession) : IRoleService
    {
        private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IJwtSession _jwtSession = jwtSession;

        public async Task AssignUsersToRoleAsync(Guid roleId, IEnumerable<string> userNames, CancellationToken cancellationToken = default)
        {
            _jwtSession.IsUserSuperAdmin(true);

            var role = await _roleManager.Roles
                                .Include(r => r.UserRoles)
                                    .ThenInclude(ur => ur.User)
                                .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

            if (role is null) throw new NotFoundException("Role is not found.");

            var existingUserNames = role.UserRoles.Select(ur => ur.User.UserName).ToHashSet();

            var usersToAdd = _userManager.Users.Where(u => userNames.Contains(u.UserName) && !existingUserNames.Contains(u.UserName));

            var usersToRemove = role.UserRoles.Where(ur => !userNames.Contains(ur.User.UserName)).ToList();

            foreach (var user in usersToAdd)
            {
                role.UserRoles.Add(new ApplicationUserRole { UserId = user.Id, RoleId = role.Id });
            }

            foreach (var userRole in usersToRemove)
            {
                role.UserRoles.Remove(userRole);
            }

            var result = await _roleManager.UpdateAsync(role);

            if (!result.Succeeded)
            {
                throw new UpdateNotSucceededException($"Role update failed: {Helpers.GetIdentityResultError(result)}");
            }
        }

        public async Task<Guid> CreateAsync(CreateRoleDTO createRoleDTO, CancellationToken cancellationToken = default)
        {
            _jwtSession.IsUserSuperAdmin(true);

            var existingRole = await _roleManager.FindByNameAsync(createRoleDTO.Name);

            if (existingRole is not null) throw new ValidationException($"Role with the name '{existingRole.Name}' is already exists.");

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

            if (role is null) throw new NotFoundException("Role is not found.");

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

            var role = await _roleManager.Roles
                                .Include(r => r.UserRoles)
                                    .ThenInclude(ur => ur.User)
                                .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

            if (role is null) throw new NotFoundException("Role is not found.");

            return role.UserRoles.Select(ur => new UserDTO(ur.User));
        }

        public async Task<Guid> UpdateAsync(Guid roleId, UpdateRoleDTO updateRoleDTO, CancellationToken cancellationToken = default)
        {
            _jwtSession.IsUserSuperAdmin(true);

            var role = await _roleManager.FindByIdAsync(roleId.ToString());

            if (role is null) throw new NotFoundException("Role is not found.");

            if (!string.IsNullOrEmpty(updateRoleDTO.Name) && updateRoleDTO.Name != role.Name)
            {
                var existingRole = await _roleManager.FindByNameAsync(updateRoleDTO.Name);

                if (existingRole is not null) throw new ValidationException($"Role with the name '{existingRole.Name}' is already exists.");

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
