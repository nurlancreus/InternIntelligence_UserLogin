namespace InternIntelligence_UserLogin.Core.Abstractions.Mail
{
    public interface IUserEmailService
    {
        Task SendWelcomeEmailAsync(string userName, string email);
        Task SendAccountConfirmationEmailAsync(string userId, string userName, string email, string confirmationToken);
        Task SendResetPasswordEmailAsync(string userId, string userName, string email, string resetToken);
    }
}
