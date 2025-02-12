namespace InternIntelligence_UserLogin.Core.Options.Email
{
    public class EmailSettings
    {
        public string From { get; set; } = string.Empty;
        public string SmtpServer { get; set; } = string.Empty;
        public int Port { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string AccountConfirmationBaseUrl { get; set; } = string.Empty;
        public string ResetPasswordBaseUrl { get; set; } = string.Empty;
    }
}
