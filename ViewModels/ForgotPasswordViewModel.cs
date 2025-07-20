using System.ComponentModel.DataAnnotations;

namespace TechBoard.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}