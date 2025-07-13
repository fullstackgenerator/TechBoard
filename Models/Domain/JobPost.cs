namespace TechBoard.Models.Domain;

public class JobPost : BaseEntity
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Requirements { get; set; } = null!;
    public string Location { get; set; } = null!;
    public bool IsRemote { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public DateTime PostedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public Category Category { get; set; }
    public JobLevel JobLevel { get; set; }
    public WorkType WorkType { get; set; }
    public string? Benefits { get; set; }
    public int ViewCount { get; set; }
    
    public bool IsFeatured { get; set; }

    // foreign key
    public string CompanyId { get; set; } = null!;
    
    public Company Company { get; set; } = null!;
    public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
}