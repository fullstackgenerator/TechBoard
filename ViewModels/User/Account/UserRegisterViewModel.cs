using System.ComponentModel.DataAnnotations;

namespace TechBoard.ViewModels.User.Account;

public class UserRegisterViewModel
{
    [Required]
    [StringLength(20, MinimumLength = 2)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = null!;

    [Required]
    [StringLength(20, MinimumLength = 2)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = null!;
    
    [Required]
    [StringLength(30, MinimumLength = 7)]
    [Display(Name = "Address")]
    public string Address { get; set; } = null!;

    [Required]
    [StringLength(10, MinimumLength = 3)]
    [Display(Name = "Postal Code")]
    public string PostalCode { get; set; } = null!;

    [Required]
    [StringLength(20, MinimumLength = 2)]
    [Display(Name = "City")]
    public string City { get; set; } = null!;

    [Required]
    [StringLength(30, MinimumLength = 2)]
    [Display(Name = "Country")]
    public string Country { get; set; } = null!;

    [Required]
    [Phone]
    [StringLength(20, MinimumLength = 5)]
    [Display(Name = "Phone")]
    public string Phone { get; set; } = null!;

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
    
    public string? Website { get; set; }
}