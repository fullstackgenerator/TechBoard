using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace TechBoard.Services
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
    }

    public class RealEmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<RealEmailService> _logger;

        public RealEmailService(IOptions<EmailSettings> emailSettings, ILogger<RealEmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                var mail = new MailMessage()
                {
                    From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true,
                };

                mail.To.Add(new MailAddress(email));

                var smtp = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword),
                    EnableSsl = true,
                };

                await smtp.SendMailAsync(mail);
                _logger.LogInformation($"Email sent successfully to {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email to {email}");
                throw;
            }
        }

        public Task SendConfirmationEmailAsync(string email, string confirmationLink)
        {
            var message = $@"
                <h2>Confirm your account</h2>
                <p>Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.</p>";
            
            return SendEmailAsync(email, "Confirm your account - TechBoard", message);
        }

        public Task SendPasswordResetEmailAsync(string email, string resetLink)
        {
            var message = $@"
                <h2>Reset your password</h2>
                <p>Please reset your password by <a href='{resetLink}'>clicking here</a>.</p>
                <p>This link will expire in 24 hours.</p>
                <p>If you did not request this password reset, please ignore this email.</p>";
            
            return SendEmailAsync(email, "Reset your password - TechBoard", message);
        }
    }
}