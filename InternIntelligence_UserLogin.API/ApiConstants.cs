namespace InternIntelligence_UserLogin.API
{
    public static class ApiConstants
    {
        public static class AuthPolicies
        {
            public const string AdminsPolicy = "Admins";
            public const string SuperAdminPolicy = "SuperAdmin";
            public const string UserOrAdminPolicy = "UserOrAdmin";
            public const string UserPolicy = "User";
        }

        public static class CorsPolicies
        {
            public const string AllowAllPolicy = "AllowAll";
        }
    }
}
