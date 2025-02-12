using InternIntelligence_UserLogin.Core.DTOs.Mail;

namespace InternIntelligence_UserLogin.Core.Abstractions.Mail
{
    public interface IEmailService
    {
        Task SendEmailAsync(RecipientDetailsDTO recipientDetails, string subject, string body);
    }
}
