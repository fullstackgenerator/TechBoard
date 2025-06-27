namespace TechBoard.ViewModels.Admin.Dashboard;

public class AdminDashboardViewModel
{
    public int TotalCompanies { get; set; }
    public int TotalJobPosts { get; set; }
    public int TotalUsers { get; set; }

    public IEnumerable<string> RecentActivities { get; set; } = Enumerable.Empty<string>();
}