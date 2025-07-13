using System.ComponentModel.DataAnnotations;

namespace TechBoard.ViewModels.User.Profile;

public class EditUserProfileViewModel
{
    [Required]
    [StringLength(20, MinimumLength = 2)]
    [Display(Name = "First Name")]
    public string? FirstName { get; set; }

    [Required]
    [StringLength(20, MinimumLength = 2)]
    [Display(Name = "Last Name")]
    public string? LastName { get; set; }

    [Required]
    [StringLength(30, MinimumLength = 7)]
    [Display(Name = "Address")]
    public string? Address { get; set; }

    [Required]
    [StringLength(10, MinimumLength = 3)]
    [Display(Name = "Postal Code")]
    public string? PostalCode { get; set; }

    [Required]
    [StringLength(20, MinimumLength = 2)]
    [Display(Name = "City")]
    public string? City { get; set; }

    [Required]
    [StringLength(30, MinimumLength = 2)]
    [Display(Name = "Country")]
    public string? Country { get; set; }

    [Required]
    [Phone]
    [StringLength(20, MinimumLength = 5)]
    [Display(Name = "Phone")]
    public string? Phone { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(50, MinimumLength = 5)]
    [Display(Name = "Email")]
    public string Email { get; set; } = null!;

    [StringLength(80, MinimumLength = 5)]
    [Display(Name = "LinkedIn")]
    public string? LinkedInProfile { get; set; }

    [StringLength(80, MinimumLength = 5)]
    [Display(Name = "GitHub")]
    public string? GitHubProfile { get; set; }

    [StringLength(80, MinimumLength = 5)]
    [Display(Name = "Website")]
    public string? Website { get; set; }
}