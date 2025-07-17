namespace TechBoard.ViewModels.Admin.Dashboard;

public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalCompanies { get; set; }
    public int TotalJobPosts { get; set; }
    public int TotalJobApplications { get; set; }

    // lists for management
    public IEnumerable<UserManagementViewModel> Users { get; set; } = new List<UserManagementViewModel>();
    public IEnumerable<CompanyManagementViewModel> Companies { get; set; } = new List<CompanyManagementViewModel>();
    public IEnumerable<JobPostManagementViewModel> JobPosts { get; set; } = new List<JobPostManagementViewModel>();
}

// helper ViewModels for lists
public class UserManagementViewModel
{
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public bool IsCompany { get; set; }
    public bool IsBlocked { get; set; }
    public int JobApplicationsCount { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CompanyManagementViewModel
{
    public string Id { get; set; } = null!;
    public string CompanyName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool IsBlocked { get; set; }
    public int TotalJobPosts { get; set; }
    public int ActiveJobPosts { get; set; }
    public int TotalApplicationsReceived { get; set; }
    public string MembershipTier { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
}

public class JobPostManagementViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string CompanyName { get; set; } = null!;
    public string CompanyId { get; set; } = null!;
    public DateTime PostedDate { get; set; }
    public bool IsActive { get; set; }
    public int ViewCount { get; set; }
    public int ApplicationsCount { get; set; }
    public bool IsFeatured { get; set; }
}