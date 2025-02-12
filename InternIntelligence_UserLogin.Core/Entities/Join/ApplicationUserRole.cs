using Microsoft.AspNetCore.Identity;

namespace InternIntelligence_UserLogin.Core.Entities.Join
{
    public class ApplicationUserRole : IdentityUserRole<Guid>
    {
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ApplicationRole Role { get; set; } = null!;
    }
}
