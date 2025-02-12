using InternIntelligence_UserLogin.Core.Abstractions.Session;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace InternIntelligence_UserLogin.Infrastructure.Services.Session
{
    public class JwtSession : IJwtSession
    {
        private enum UserClaimType : byte { Id, Email, UserName }

        private readonly ClaimsPrincipal? claimsPrincipal;

        public JwtSession(IHttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor.HttpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContextAccessor.HttpContext), "HttpContext is null.");
            }

            claimsPrincipal = httpContextAccessor.HttpContext.User;
        }

        public bool IsUserAuthenticated(bool throwException = false)
        {
            bool isUserAuthenticated = claimsPrincipal?.Identity?.IsAuthenticated ?? false;

            if (!isUserAuthenticated && throwException)
                throw new UnauthorizedAccessException("User is not authenticated. Please log in to access this resource.");

            return isUserAuthenticated;
        }

        public bool IsUserAuthorized(Guid id, bool throwException = false)
        {
            var userId = GetUserId();

            if (userId != id.ToString())
            {
                if (throwException) throw new UnauthorizedAccessException("User is not authorized to do this action.");

                return false;
            }

            return true;
        }

        public bool IsUserAuthorizedOrAdmin(Guid id, bool throwException = true)
        {
            var isAuth = IsUserAuthorized(id) || IsUserAdmin();

            if (!isAuth)
            {
                if (throwException) throw new UnauthorizedAccessException("User is not authorized to do this action.");

                return false;
            }

            return true;
        }

        public string GetUserEmail() => GetUserClaim(UserClaimType.Email);

        public string GetUserId() => GetUserClaim(UserClaimType.Id);

        public string GetUserName() => GetUserClaim(UserClaimType.UserName);

        public IEnumerable<string> GetUserRoles()
        {
            IsUserAuthenticated();

            return claimsPrincipal?
                .FindAll(claim => claim.Type == ClaimTypes.Role)
                .Select(claim => claim.Value) ?? [];
        }

        public bool IsRolesDefined(IEnumerable<string> roles, bool throwException = false)
        {
            var userRoles = GetUserRoles().ToHashSet();

            if (roles.All(userRoles.Contains))
            {
                return true;
            }

            if (throwException)
            {
                throw new UnauthorizedAccessException("User does not have the required roles to access this resource.");
            }

            return false;
        }

        public bool IsUserAdmin(bool throwException = false)
        {
            var isAuth = IsRolesDefined(["Admin"]) || IsUserSuperAdmin();

            if (!isAuth)
            {
                if (throwException) throw new UnauthorizedAccessException("User is not authorized to do this action.");

                return false;
            }

            return true;
        }

        public bool IsUserSuperAdmin(bool throwException = false) => IsRolesDefined(["SuperAdmin"], throwException);

        private string GetUserClaim(UserClaimType claimType)
        {
            IsUserAuthenticated(true);

            string? claimValue = claimType switch
            {
                UserClaimType.Id => claimsPrincipal!.FindFirstValue(ClaimTypes.NameIdentifier),
                UserClaimType.Email => claimsPrincipal!.FindFirstValue(ClaimTypes.Email),
                UserClaimType.UserName => claimsPrincipal!.FindFirstValue(ClaimTypes.Name),
                _ => throw new InvalidOperationException("Invalid claim type specified."),
            };

            if (claimValue is null)
            {
                throw new UnauthorizedAccessException($"Claim of type '{claimType}' is not found in the user's claims.");
            }

            return claimValue;
        }
    }
}