using Microsoft.AspNetCore.Identity;

namespace InternIntelligence_UserLogin.Core.Entities.Join
{
    public class ApplicationUserClaim : IdentityUserClaim<Guid>
    {
        public ApplicationUser User { get; set; } = null!;
    }
}
