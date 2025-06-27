using System.ComponentModel.DataAnnotations;

namespace TechBoard.ViewModels.Admin.Account;

public class AdminRegisterViewModel
{
    [Required]
    [StringLength(20, MinimumLength = 5)]
    [Display(Name = "Full Name")]
    public string Name { get; set; } = null!;
    
    [Required]
    [EmailAddress]
    [StringLength(50, MinimumLength = 5)]
    [Display(Name = "Email")]
    public string Email { get; set; } = null!;
    
    [Required]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
    [Display(Name = "Password")]
    public string Password { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = null!;
}