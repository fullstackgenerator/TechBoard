using System.ComponentModel.DataAnnotations;

namespace TechBoard.ViewModels.Company;

public class CompanyProfileViewModel
{
    [Required]
    [StringLength(20, MinimumLength = 5)]
    [Display(Name = "Company Name")]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(20, MinimumLength = 5)]
    [Display(Name = "Business registration number")]
    public string IdNumber { get; set; } = null!;
    
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
}