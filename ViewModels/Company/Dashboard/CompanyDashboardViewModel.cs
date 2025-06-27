using TechBoard.ViewModels.Company.Profile;
using TechBoard.ViewModels.JobPost;

namespace TechBoard.ViewModels.Company.Dashboard;

public class CompanyDashboardViewModel
{
    public CompanyProfileViewModel Profile { get; set; } = null!;
    public IEnumerable<JobPostViewModel> JobPosts { get; set; } = Enumerable.Empty<JobPostViewModel>();
}

