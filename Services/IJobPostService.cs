using TechBoard.Models.Domain;

namespace TechBoard.Services;

public interface IJobPostService
{
    Task<IEnumerable<JobPost>> GetAllJobPostsAsync();
    Task<JobPost?> GetJobPostByIdAsync(int id);
    Task IncrementJobPostViewCountAsync(int id);
    Task<IEnumerable<JobPost>> GetJobPostsByCompanyAsync(string companyId);
    Task<JobPost> CreateJobPostAsync(JobPost jobPost, string companyId);
    Task<JobPost> UpdateJobPostAsync(JobPost jobPost);
    Task<bool> DeleteJobPostAsync(int id, string companyId);
    Task<bool> CanCompanyPostMoreJobsAsync(string companyId);
    Task<IEnumerable<JobPost>> SearchJobPostsAsync(string searchTerm, Category? category = null, string? location = null);
}