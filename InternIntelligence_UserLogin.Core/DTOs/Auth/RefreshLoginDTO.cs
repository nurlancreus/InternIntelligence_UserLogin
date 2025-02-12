using System.ComponentModel.DataAnnotations;

namespace InternIntelligence_UserLogin.Core.DTOs.Auth
{
    public record RefreshLoginDTO
    {
        [Required(ErrorMessage = "AccessToken is required.")]
        public string AccessToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "RefreshToken is required.")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
