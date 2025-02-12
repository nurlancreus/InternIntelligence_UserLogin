using InternIntelligence_UserLogin.Core.Abstractions.Base;
using InternIntelligence_UserLogin.Core.Entities.Join;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternIntelligence_UserLogin.Core.Entities
{
    public class ApplicationRole : IdentityRole<Guid>, IAuditable
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ICollection<ApplicationUserRole> UserRoles { get; set; } = [];
        public ICollection<ApplicationRoleClaim> RoleClaims { get; set; } = [];

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
