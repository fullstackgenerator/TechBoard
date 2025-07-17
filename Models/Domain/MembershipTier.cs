namespace TechBoard.Models.Domain;

public class MembershipTier : BaseEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public int MaxJobPosts { get; set; }
    public int JobPostDurationDays { get; set; }
    public bool CanPostFeatured { get; set; }
    public bool CanAccessAnalytics { get; set; }
    public bool CanContactCandidates { get; set; }
    public int MaxApplicationsPerJob { get; set; }
    
    public ICollection<Company> Companies { get; set; } = new List<Company>();
}