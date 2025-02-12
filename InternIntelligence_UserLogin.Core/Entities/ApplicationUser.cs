using InternIntelligence_UserLogin.Core.Abstractions.Base;
using InternIntelligence_UserLogin.Core.Entities.Join;
using Microsoft.AspNetCore.Identity;

namespace InternIntelligence_UserLogin.Core.Entities
{
    public class ApplicationUser : IdentityUser<Guid>, IAuditable
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenEndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<ApplicationUserClaim> Claims { get; set; } = [];
        public virtual ICollection<ApplicationUserLogin> Logins { get; set; } = [];
        public virtual ICollection<ApplicationUserToken> Tokens { get; set; } = [];
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = [];

        private ApplicationUser() { }
        private ApplicationUser(string firstName, string lastName, string userName, string email)
        {
            FirstName = firstName;
            LastName = lastName;
            UserName = userName;
            Email = email;
        }

        public static ApplicationUser Create(string firstName, string lastName, string userName, string email)
        {
            return new ApplicationUser(firstName, lastName, userName, email);
        }
    }
}
