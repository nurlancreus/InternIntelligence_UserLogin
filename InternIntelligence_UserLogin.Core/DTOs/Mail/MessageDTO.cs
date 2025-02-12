using MimeKit;

namespace InternIntelligence_UserLogin.Core.DTOs.Mail
{
    public record MessageDTO
    {
        public MailboxAddress? To { get; set; }
        public List<MailboxAddress> Recipients { get; set; } = [];
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
