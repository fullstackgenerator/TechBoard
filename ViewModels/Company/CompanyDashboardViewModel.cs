using TechBoard.Models.Domain;

namespace TechBoard.ViewModels.Company;

public class CompanyDashboardViewModel
{
    public CompanyProfile Company { get; set; } = null!;
    public IEnumerable<JobPost> JobPosts { get; set; } = Enumerable.Empty<JobPost>();
}

