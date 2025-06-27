using System.ComponentModel.DataAnnotations;

namespace TechBoard.ViewModels.Admin.Account;

public class AdminLoginViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = null!;
   
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }  = null!;
    
    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }
}