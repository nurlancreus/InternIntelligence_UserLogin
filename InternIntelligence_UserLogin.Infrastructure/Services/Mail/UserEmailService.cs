using InternIntelligence_UserLogin.Core.Abstractions.Services.Mail;
using InternIntelligence_UserLogin.Core.DTOs.Mail;
using InternIntelligence_UserLogin.Core.Options.Email;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Text;

namespace InternIntelligence_UserLogin.Infrastructure.Services.Mail
{
    public class UserEmailService(IOptions<EmailSettings> options, IEmailService emailService, IEmailTemplateService emailTemplateService) : IUserEmailService
    {
        private readonly EmailSettings _emailSettings = options.Value;

        private readonly IEmailService _emailService = emailService;
        private readonly IEmailTemplateService _emailTemplateService = emailTemplateService;

        public async Task SendAccountConfirmationEmailAsync(Guid userId, string userName, string email, string confirmationToken)
        {
            var recipientDetails = GenerateRecipient(userName, email);

            var encodedToken = confirmationToken.UrlEncode();

            var confirmationLink = $"{_emailSettings.AccountConfirmationBaseUrl}?userId={userId}&token={encodedToken}";

            var body = _emailTemplateService.GenerateAccountConfirmationEmail(userName, confirmationLink);

            await _emailService.SendEmailAsync(recipientDetails, "Account Confirmation", body);
        }

        public async Task SendResetPasswordEmailAsync(Guid userId, string userName, string email, string resetToken)
        {
            var recipientDetails = GenerateRecipient(userName, email);

            var encodedToken = resetToken.UrlEncode();

            var resetLink = $"{_emailSettings.ResetPasswordBaseUrl}?userId={userId}&token={encodedToken}";

            var body = _emailTemplateService.GeneratePasswordResetEmail(userName, resetLink);

            await _emailService.SendEmailAsync(recipientDetails, "Password Reset", body);
        }

        public async Task SendWelcomeEmailAsync(string userName, string email)
        {
            var recipientDetails = GenerateRecipient(userName, email);

            var body = _emailTemplateService.GenerateWelcomeEmail(userName);

            await _emailService.SendEmailAsync(recipientDetails, "Welcome", body);
        }

        private static RecipientDetailsDTO GenerateRecipient(string userName, string email) => new() { Name = userName, Email = email };

    }
}
