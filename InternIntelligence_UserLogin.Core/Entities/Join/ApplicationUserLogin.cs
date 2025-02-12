using Microsoft.AspNetCore.Identity;

namespace InternIntelligence_UserLogin.Core.Entities.Join
{
    public class ApplicationUserLogin : IdentityUserLogin<Guid>
    {
        public ApplicationUser User { get; set; } = null!;
    }
}
