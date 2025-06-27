namespace TechBoard.Models.Domain;

public class JobPost : BaseEntity
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Location { get; set; } = null!;
    public DateTime PostedDate { get; set; }

    public int CompanyProfileId { get; set; }
    public Company Company { get; set; } = null!;
}
