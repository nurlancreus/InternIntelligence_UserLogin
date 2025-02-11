namespace InternIntelligence_UserLogin.Core.Abstractions.Mail
{
    public interface IEmailTemplateService
    {
        string GenerateWelcomeEmail(string userName);
        string GeneratePasswordResetEmail(string userName, string resetLink);
        string GenerateAccountConfirmationEmail(string userName, string confirmationLink);
    }
}
