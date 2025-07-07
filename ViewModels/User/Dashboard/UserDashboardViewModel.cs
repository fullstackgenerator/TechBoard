using TechBoard.ViewModels.User.Profile;

namespace TechBoard.ViewModels.User.Dashboard;

public class UserDashboardViewModel
{
    public UserProfileViewModel Profile { get; set; } = null!;
    public JobApplicationViewModel JobApplication { get; set; } = null!;
}

