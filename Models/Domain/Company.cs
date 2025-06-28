namespace TechBoard.Models.Domain;

public class Company : ApplicationUser
{
    public string Address { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Country { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string IdNumber { get; set; } = null!;
    public string? Website { get; set; }
    public string? Description { get; set; }
    public int? EmployeeCount { get; set; }
    
    // foreign key
    public int MembershipTierId { get; set; }
    
    public MembershipTier MembershipTier { get; set; } = null!;
    public ICollection<JobPost> JobPosts { get; set; } = new List<JobPost>();
}