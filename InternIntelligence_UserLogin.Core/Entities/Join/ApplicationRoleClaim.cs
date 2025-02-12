using Microsoft.AspNetCore.Identity;

namespace InternIntelligence_UserLogin.Core.Entities.Join
{
    public class ApplicationRoleClaim : IdentityRoleClaim<Guid>
    {
        public ApplicationRole Role { get; set; } = null!;
    }
}
