namespace TechBoard.Models.Domain;

public class JobPost
{
    public int CompanyProfileId { get; set; }
    public Company Company { get; set; } = null!;
}