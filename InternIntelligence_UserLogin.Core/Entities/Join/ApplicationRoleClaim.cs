using Microsoft.AspNetCore.Identity;

namespace InternIntelligence_UserLogin.Core.Entities.Join
{
    public class ApplicationRoleClaim : IdentityRoleClaim<Guid>
    {
        public virtual ApplicationRole Role { get; set; } = null!;
    }
}
