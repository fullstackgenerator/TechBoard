namespace TechBoard.ViewModels.PublicJobPost;

public class JobPostsListViewModel
{
    public IEnumerable<PublicJobPostViewModel> JobPosts { get; set; } = new List<PublicJobPostViewModel>();
    public PublicJobPostViewModel.JobPostFilterViewModel Filters { get; set; } = new PublicJobPostViewModel.JobPostFilterViewModel();
    
    public List<string> AvailableCategories { get; set; } = new List<string>();
    public List<string> AvailableJobLevels { get; set; } = new List<string>();
    public List<string> AvailableWorkTypes { get; set; } = new List<string>();
    public List<string> AvailableLocations { get; set; } = new List<string>();
}