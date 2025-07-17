using System.ComponentModel.DataAnnotations;

namespace TechBoard.ViewModels.Admin.Profile;

public class EditAdminProfileViewModel
{
    [Required]
    [StringLength(50, MinimumLength = 5)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = null!;

    [Required]
    [EmailAddress]
    [StringLength(50, MinimumLength = 5)]
    [Display(Name = "Email")]
    public string Email { get; set; } = null!;
}