using InternIntelligence_UserLogin.Core.Abstractions.Services;
using InternIntelligence_UserLogin.Core.Abstractions.Services.Mail;
using InternIntelligence_UserLogin.Core.Abstractions.Session;
using InternIntelligence_UserLogin.Core.DTOs.Role;
using InternIntelligence_UserLogin.Core.DTOs.User;
using InternIntelligence_UserLogin.Core.Entities;
using InternIntelligence_UserLogin.Core.Exceptions;
using InternIntelligence_UserLogin.Core.Options.Token;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text;

namespace InternIntelligence_UserLogin.Infrastructure.Persistence.Services
{
    public class UserService(IOptions<TokenSettings> options, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IJwtSession jwtSession, IUserEmailService userEmailService) : IUserService
    {
        private readonly RefreshSettings _refreshSettings = options.Value.Refresh;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
        private readonly IJwtSession _jwtSession = jwtSession;
        private readonly IUserEmailService _userEmailService = userEmailService;

        public async Task AssignRolesToUserAsync(Guid userId, IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default)
        {
            _jwtSession.IsUserSuperAdmin(true);

            var user = await _userManager.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user is null) throw new NotFoundException("User not found.");

            var existingRoleIds = user.UserRoles.Select(ur => ur.Role.Id).ToHashSet();

            var rolesToAdd = _roleManager.Roles.Where(r => roleIds.Contains(r.Id) && !existingRoleIds.Contains(r.Id));

            var rolesToRemove = user.UserRoles.Where(ur => !roleIds.Contains(ur.Role.Id)).ToList();

            foreach (var role in rolesToAdd)
            {
                user.UserRoles.Add(new ApplicationUserRole { UserId = user.Id, RoleId = role.Id });
            }

            foreach (var userRole in rolesToRemove)
            {
                user.UserRoles.Remove(userRole);
            }

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                throw new UpdateNotSucceededException($"User update failed: {Helpers.GetIdentityResultError(result)}");
            }
        }

        public async Task<IEnumerable<UserDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            _jwtSession.IsUserAdmin(true);

            var users = await _userManager.Users.Select(u => new UserDTO(u)).AsNoTracking().ToListAsync(cancellationToken: cancellationToken);

            return users;
        }

        public async Task<UserDTO> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user is null) throw new NotFoundException("User is not found.");

            _jwtSession.IsUserAuthorizedOrAdmin(user.Id);

            return new UserDTO(user);
        }

        public async Task<IEnumerable<RoleDTO>> GetUserRoles(Guid id, CancellationToken cancellationToken = default)
        {
            _jwtSession.IsUserAdmin(true);

            var user = await _userManager.Users
                                .Include(u => u.UserRoles)
                                    .ThenInclude(ur => ur.Role)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

            if (user is null) throw new NotFoundException("User is not found.");

            return user.UserRoles.Select(ur => new RoleDTO(ur.Role));
        }

        public async Task UpdateRefreshTokenAsync(ApplicationUser user, string refreshToken, DateTime accessTokenEndDate)
        {
            var refreshTokenEndDate = accessTokenEndDate.AddMinutes(_refreshSettings.RefreshTokenLifeTimeInMinutes);

            user.RefreshToken = refreshToken;
            user.RefreshTokenEndDate = refreshTokenEndDate;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                throw new UpdateNotSucceededException($"User update failed: {Helpers.GetIdentityResultError(result)}");
            }
        }

        public async Task RequestPasswordResetAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user is null) throw new NotFoundException("User not found.");

            _jwtSession.IsUserAuthorized(user.Id, true);

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            await _userEmailService.SendResetPasswordEmailAsync(userId, user.UserName!, user.Email!, resetToken);
        }

        public async Task ResetPasswordAsync(Guid userId, string token, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user is null) throw new NotFoundException("User not found.");

            _jwtSession.IsUserAuthorized(user.Id, true);

            var decodedToken = token.UrlDecode();

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, newPassword);
            if (!result.Succeeded)
            {
                throw new PasswordResetException($"Password reset failed: {Helpers.GetIdentityResultError(result)}");
            }
        }
    }
}
