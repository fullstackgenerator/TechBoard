namespace TechBoard.Models.Domain;

public class User : ApplicationUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? LinkedInProfile { get; set; }
    public string? GitHubProfile { get; set; }
    public string? Website { get; set; }
    
    public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
}