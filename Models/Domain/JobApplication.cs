namespace TechBoard.Models.Domain;

public class JobApplication : BaseEntity
{
    public string CoverLetter { get; set; } = null!;
    public string? ResumeFileName { get; set; }
    public string? ResumeFilePath { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    public DateTime AppliedDate { get; set; } = DateTime.UtcNow;
    public string? CompanyNotes { get; set; }

    // foreign keys
    public string UserId { get; set; } = null!;
    public int JobPostId { get; set; }
    
    public User User { get; set; } = null!;
    public JobPost JobPost { get; set; } = null!;
}