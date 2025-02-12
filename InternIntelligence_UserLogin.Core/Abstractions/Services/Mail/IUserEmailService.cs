namespace InternIntelligence_UserLogin.Core.Abstractions.Services.Mail
{
    public interface IUserEmailService
    {
        Task SendWelcomeEmailAsync(string userName, string email);
        Task SendAccountConfirmationEmailAsync(Guid userId, string userName, string email, string confirmationToken);
        Task SendResetPasswordEmailAsync(Guid userId, string userName, string email, string resetToken);
    }
}
