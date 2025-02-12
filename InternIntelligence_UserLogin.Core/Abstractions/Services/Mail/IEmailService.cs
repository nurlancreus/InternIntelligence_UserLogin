using InternIntelligence_UserLogin.Core.DTOs.Mail;

namespace InternIntelligence_UserLogin.Core.Abstractions.Services.Mail
{
    public interface IEmailService
    {
        Task SendEmailAsync(RecipientDetailsDTO recipientDetails, string subject, string body);
    }
}
