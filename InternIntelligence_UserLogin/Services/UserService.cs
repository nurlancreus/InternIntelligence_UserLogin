using InternIntelligence_UserLogin.Core.Abstractions;
using InternIntelligence_UserLogin.Core.Data.Entities;
using InternIntelligence_UserLogin.Core.DTOs.User;
using InternIntelligence_UserLogin.Core.Exceptions;
using InternIntelligence_UserLogin.Core.Options.Token;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace InternIntelligence_UserLogin.Services
{
    public class UserService(IOptions<TokenSettings> options, UserManager<ApplicationUser> userManager) : IUserService
    {
        private readonly RefreshSettings _refreshSettings = options.Value.Refresh;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        public async Task<IEnumerable<UserDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var users = await _userManager.Users.Select(u => new UserDTO(u)).ToListAsync(cancellationToken: cancellationToken);

            return users;
        }

        public async Task<UserDTO> GetAsync(string id, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null) throw new NotFoundException("User is not found.");

            return new UserDTO(user);
        }

        public async Task UpdateRefreshTokenAsync(ApplicationUser user, string refreshToken, DateTime accessTokenEndDate)
        {
            var refreshTokenEndDate = accessTokenEndDate.AddMinutes(_refreshSettings.RefreshTokenLifeTimeInMinutes);

            user.RefreshToken = refreshToken;
            user.RefreshTokenEndDate = refreshTokenEndDate;

            await _userManager.UpdateAsync(user);
        }
    }
}
