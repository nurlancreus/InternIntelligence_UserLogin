namespace InternIntelligence_UserLogin.Core.Abstractions.Session
{
    public interface IJwtSession
    {
        bool IsUserAuthenticated(bool throwException = false);
        bool IsUserAuthorized(Guid id, bool throwException = false);
        bool IsUserAuthorizedOrAdmin(Guid id, bool throwException = true);
        string GetUserId();
        string GetUserName();
        string GetUserEmail();
        IEnumerable<string> GetUserRoles();
        bool IsRolesDefined(IEnumerable<string> roles, bool throwException = false);
        bool IsUserAdmin(bool throwException = false);
        bool IsUserSuperAdmin(bool throwException = false);
    }
}
