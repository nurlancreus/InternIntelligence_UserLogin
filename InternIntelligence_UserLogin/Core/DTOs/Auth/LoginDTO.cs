using System.ComponentModel.DataAnnotations;

namespace InternIntelligence_UserLogin.Core.DTOs.Auth
{
    public record LoginDTO
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 20 characters.")]
        public string Password { get; set; } = string.Empty;
    }
}