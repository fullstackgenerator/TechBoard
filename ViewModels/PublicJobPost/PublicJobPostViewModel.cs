using System.ComponentModel.DataAnnotations;

namespace TechBoard.ViewModels.PublicJobPost;

public class PublicJobPostViewModel
{
    public int Id { get; set; }

    [Display(Name = "Job Title")]
    public string Title { get; set; } = null!;

    [Display(Name = "Description")]
    public string Description { get; set; } = null!;

    [Display(Name = "Location")]
    public string Location { get; set; } = null!;

    [Display(Name = "Posted On")]
    [DisplayFormat(DataFormatString = "{0:MMM dd, yyyy}")]
    public DateTime PostedDate { get; set; }

    [Display(Name = "Company")]
    public string CompanyName { get; set; } = null!;
    
    [Display(Name = "Requirements")]
    public string Requirements { get; set; } = null!;
    public string? Benefits { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string Category { get; set; } = null!;
    public string JobLevel { get; set; } = null!;
    public string WorkType { get; set; } = null!;
    public bool IsRemote { get; set; }
}