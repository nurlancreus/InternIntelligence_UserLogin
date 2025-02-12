using InternIntelligence_UserLogin.Core.Abstractions.Services.Mail;

namespace InternIntelligence_UserLogin.Infrastructure.Services.Mail
{
    public class EmailTemplateService : IEmailTemplateService
    {
        public string GenerateWelcomeEmail(string userName)
        {
            return $@"
                <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2>Welcome, {userName}!</h2>
                        <p>We're excited to have you on board. Enjoy our services!</p>
                        <p>Best Regards,<br><strong>Intern Intelligence Team</strong></p>
                    </body>
                </html>";
        }

        public string GeneratePasswordResetEmail(string userName, string resetLink)
        {
            return $@"
                <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2>Hello, {userName}</h2>
                        <p>You requested a password reset. Click the button below to reset your password:</p>
                        <p><a href='{resetLink}' style='padding: 10px; background: #007bff; color: white; text-decoration: none; border-radius: 5px;'>Reset Password</a></p>
                        <p>If you did not request this, please ignore this email.</p>
                        <p>Best Regards,<br><strong>Intern Intelligence Team</strong></p>
                    </body>
                </html>";
        }

        public string GenerateAccountConfirmationEmail(string userName, string confirmationLink)
        {
            return $@"
                <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2>Hi {userName},</h2>
                        <p>Thank you for signing up! Please confirm your email by clicking the button below:</p>
                        <p><a href='{confirmationLink}' style='padding: 10px; background: #28a745; color: white; text-decoration: none; border-radius: 5px;'>Confirm Email</a></p>
                        <p>Best Regards,<br><strong>Intern Intelligence Team</strong></p>
                    </body>
                </html>";
        }
    }
}
