using Microsoft.AspNetCore.Identity;

namespace InternIntelligence_UserLogin.Core.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenEndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

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
