using System.ComponentModel.DataAnnotations;

namespace TechBoard.ViewModels.Admin.Account;

public class ChangeAdminPasswordViewModel
{
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string OldPassword { get; set; }  = null!;
    
    [Required(ErrorMessage = "New Password is required")]
    [DataType(DataType.Password)]
    [Display(Name = "New password")]
    public string NewPassword { get; set; }  = null!;
    
    [DataType(DataType.Password)]
    [Display(Name = "Confirm new password")]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string ConfirmNewPassword { get; set; }  = null!;
}