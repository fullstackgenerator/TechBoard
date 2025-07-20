namespace TechBoard.Services
{
    public class MockEmailService : IEmailService
    {
        private readonly ILogger<MockEmailService> _logger;

        public MockEmailService(ILogger<MockEmailService> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            _logger.LogInformation("=== MOCK EMAIL ===");
            _logger.LogInformation($"To: {email}");
            _logger.LogInformation($"Subject: {subject}");
            _logger.LogInformation($"Message: {message}");
            _logger.LogInformation("==================");
            
            return Task.CompletedTask;
        }

        public Task SendConfirmationEmailAsync(string email, string confirmationLink)
        {
            var message = $"Please confirm your account by clicking this link: {confirmationLink}";
            return SendEmailAsync(email, "Confirm your account", message);
        }

        public Task SendPasswordResetEmailAsync(string email, string resetLink)
        {
            var message = $"Please reset your password by clicking this link: {resetLink}. This link will expire in 24 hours.";
            return SendEmailAsync(email, "Reset your password", message);
        }
    }
}