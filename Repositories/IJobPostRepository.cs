using TechBoard.Models.Domain;

namespace TechBoard.Repositories;

public interface IJobPostRepository
{
    Task<IEnumerable<JobPost>> GetAllAsync();
    Task<JobPost?> GetByIdAsync(int id);
    Task<IEnumerable<JobPost>> GetByCompanyIdAsync(string companyId);
    Task<IEnumerable<JobPost>> GetActiveByCategoryAsync(Category category);
    Task<IEnumerable<JobPost>> SearchAsync(string searchTerm, Category? category = null, string? location = null);
    Task<JobPost> CreateAsync(JobPost jobPost);
    Task<JobPost> UpdateAsync(JobPost jobPost);
    Task<bool> DeleteAsync(int id);
    Task<int> GetActiveCountByCompanyAsync(string companyId);
    Task IncrementViewCountAsync(int jobPostId);
}