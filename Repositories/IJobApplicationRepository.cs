using TechBoard.Models.Domain;

namespace TechBoard.Repositories;

public interface IJobApplicationRepository
{
    Task<IEnumerable<JobApplication>> GetByJobPostIdAsync(int jobPostId);
    Task<IEnumerable<JobApplication>> GetByUserIdAsync(string userId);
    Task<JobApplication?> GetByIdAsync(int id);
    Task<JobApplication> CreateAsync(JobApplication application);
    Task<JobApplication> UpdateAsync(JobApplication application);
}