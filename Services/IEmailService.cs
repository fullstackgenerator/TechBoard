namespace TechBoard.Services;

public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, string message);
    Task SendConfirmationEmailAsync(string email, string confirmationLink);
    Task SendPasswordResetEmailAsync(string email, string resetLink);
}