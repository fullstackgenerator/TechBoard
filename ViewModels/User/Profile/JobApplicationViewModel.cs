
namespace TechBoard.ViewModels.User.Profile
{
    public class JobApplicationViewModel
    {
        public int Id { get; set; }
        public string JobTitle { get; set; } = null!;
        public string CompanyName { get; set; } = null!;
        public string CoverLetter { get; set; } = null!;
        public string? ResumeFileName { get; set; }
        public string Status { get; set; } = null!;
        public DateTime AppliedDate { get; set; }
        public string? CompanyNotes { get; set; }
        public int JobPostId { get; set; }
    }
}