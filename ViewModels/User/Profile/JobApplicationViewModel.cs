// Assuming this is your TechBoard.ViewModels.User.Profile.JobApplicationViewModel
namespace TechBoard.ViewModels.User.Profile
{
    public class JobApplicationViewModel
    {
        public int Id { get; set; }
        public string JobTitle { get; set; } = null!;
        public string CompanyName { get; set; } = null!;
        public string? CoverLetter { get; set; }
        public string? CvFileName { get; set; }
        public string? ResumeFileName { get; set; }
        public DateTime AppliedDate { get; set; }
        public string Status { get; set; } = null!;
        public string? CompanyNotes { get; set; }
        public int JobPostId { get; set; }
    }
}