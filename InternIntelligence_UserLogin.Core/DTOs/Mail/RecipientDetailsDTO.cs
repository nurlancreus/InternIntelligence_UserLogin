namespace InternIntelligence_UserLogin.Core.DTOs.Mail
{
    public record RecipientDetailsDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
