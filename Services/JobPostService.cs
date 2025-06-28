using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechBoard.Models.Domain;
using TechBoard.Repositories;

namespace TechBoard.Services;

public class JobPostService : IJobPostService
{
    private readonly IJobPostRepository _jobPostRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public JobPostService(IJobPostRepository jobPostRepository, UserManager<ApplicationUser> userManager)
    {
        _jobPostRepository = jobPostRepository;
        _userManager = userManager;
    }

    public async Task<IEnumerable<JobPost>> GetAllJobPostsAsync()
    {
        return await _jobPostRepository.GetAllAsync();
    }

    public async Task<JobPost?> GetJobPostByIdAsync(int id)
    {
        var jobPost = await _jobPostRepository.GetByIdAsync(id);
        if (jobPost != null)
        {
            await _jobPostRepository.IncrementViewCountAsync(id);
        }
        return jobPost;
    }

    public async Task<IEnumerable<JobPost>> GetJobPostsByCompanyAsync(string companyId)
    {
        return await _jobPostRepository.GetByCompanyIdAsync(companyId);
    }

    public async Task<JobPost> CreateJobPostAsync(JobPost jobPost, string companyId)
    {
        jobPost.CompanyId = companyId;
        jobPost.PostedDate = DateTime.UtcNow;
        jobPost.IsActive = true;
        
        return await _jobPostRepository.CreateAsync(jobPost);
    }

    public async Task<JobPost> UpdateJobPostAsync(JobPost jobPost)
    {
        return await _jobPostRepository.UpdateAsync(jobPost);
    }

    public async Task<bool> DeleteJobPostAsync(int id, string companyId)
    {
        var jobPost = await _jobPostRepository.GetByIdAsync(id);
        if (jobPost == null || jobPost.CompanyId != companyId)
            return false;

        return await _jobPostRepository.DeleteAsync(id);
    }

    public async Task<bool> CanCompanyPostMoreJobsAsync(string companyId)
    {
        var user = await _userManager.Users
            .Include(u => ((Company)u).MembershipTier)
            .FirstOrDefaultAsync(u => u.Id == companyId);
            
        if (user is not Company company) 
            return false;

        var activeJobCount = await _jobPostRepository.GetActiveCountByCompanyAsync(companyId);
        return company.MembershipTier.MaxJobPosts == -1 || activeJobCount < company.MembershipTier.MaxJobPosts;
    }

    public async Task<IEnumerable<JobPost>> SearchJobPostsAsync(string searchTerm, Category? category = null, string? location = null)
    {
        return await _jobPostRepository.SearchAsync(searchTerm, category, location);
    }
}