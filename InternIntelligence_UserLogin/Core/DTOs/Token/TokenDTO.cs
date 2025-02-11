namespace InternIntelligence_UserLogin.Core.DTOs.Token
{
    public record TokenDTO
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime AccessTokenEndDate { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
    }
}
