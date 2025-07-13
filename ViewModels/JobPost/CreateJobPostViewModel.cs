using System.ComponentModel.DataAnnotations;
using TechBoard.Models.Domain;

namespace TechBoard.ViewModels.JobPost;

public class CreateJobPostViewModel
{
    [Required(ErrorMessage = "Job Title is required.")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters.")]
    [Display(Name = "Job Title")]
    public string Title { get; set; } = null!;

    [Required(ErrorMessage = "Description is required.")]
    [MinLength(50, ErrorMessage = "Description must be at least 50 characters.")]
    [Display(Name = "Job Description")]
    public string Description { get; set; } = null!;

    [Required(ErrorMessage = "Requirements are required.")]
    [MinLength(20, ErrorMessage = "Requirements must be at least 20 characters.")]
    [Display(Name = "Job Requirements")]
    public string Requirements { get; set; } = null!;

    [Required(ErrorMessage = "Location is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Location must be between 2 and 100 characters.")]
    [Display(Name = "Location")]
    public string Location { get; set; } = null!;

    [Display(Name = "Remote work?")]
    public bool IsRemote { get; set; }

    [Range(0, 9999999.99, ErrorMessage = "Salary minimum must be a positive number.")]
    [Display(Name = "Salary (Min)")]
    public decimal? SalaryMin { get; set; }

    [Range(0, 9999999.99, ErrorMessage = "Salary maximum must be a positive number.")]
    [Display(Name = "Salary (Max)")]
    public decimal? SalaryMax { get; set; }

    [Display(Name = "Job Category")]
    [Required(ErrorMessage = "Job Category is required.")]
    public Category Category { get; set; }

    [Display(Name = "Job Level")]
    [Required(ErrorMessage = "Job Level is required.")]
    public JobLevel JobLevel { get; set; }

    [Display(Name = "Work Type")]
    [Required(ErrorMessage = "Work Type is required.")]
    public WorkType WorkType { get; set; }

    [Display(Name = "Benefits (Optional)")]
    public string? Benefits { get; set; }
    
    [Display(Name = "Feature this Job Post")]
    public bool IsFeatured { get; set; }
}