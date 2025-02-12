using Microsoft.AspNetCore.Identity;

namespace InternIntelligence_UserLogin.Core.Entities.Join
{
    public class ApplicationUserToken : IdentityUserToken<Guid>
    {
        public ApplicationUser User { get; set; } = null!;
    }
}
