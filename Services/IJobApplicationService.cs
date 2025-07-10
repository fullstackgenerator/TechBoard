using TechBoard.Models.Domain;
using TechBoard.ViewModels.JobApplication;

namespace TechBoard.Services
{
    public interface IJobApplicationService
    {
        Task<(bool Success, string Message)> ApplyForJobAsync(
            int jobPostId,
            string userId,
            SubmitJobApplicationViewModel model);

        Task<IEnumerable<JobApplication>> GetApplicationsByUserIdAsync(string userId);
        Task<JobApplication?> GetApplicationDetailsAsync(int applicationId);
        
        Task<bool> HasUserAppliedAsync(string userId, int jobPostId);
    }
}