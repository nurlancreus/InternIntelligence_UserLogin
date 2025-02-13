using InternIntelligence_UserLogin.Core.Abstractions.Base;
using Microsoft.AspNetCore.Identity;

namespace InternIntelligence_UserLogin.Core.Entities
{
    public class ApplicationRole : IdentityRole<Guid>, IAuditable
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = [];

        private ApplicationRole() { }
        private ApplicationRole(string name)
        {
            Name = name;
        }

        public static ApplicationRole Create(string name)
        {
            return new ApplicationRole(name);
        }
    }
}
