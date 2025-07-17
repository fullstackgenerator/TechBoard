using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechBoard.Data;
using TechBoard.Models.Domain;
using TechBoard.Repositories;

namespace TechBoard.Services;

public class JobPostService : IJobPostService
{
    private readonly IJobPostRepository _jobPostRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<JobPostService> _logger;
    private readonly ApplicationDbContext _context;

    public JobPostService(IJobPostRepository jobPostRepository, UserManager<ApplicationUser> userManager, ILogger<JobPostService> logger, ApplicationDbContext context)
    {
        _jobPostRepository = jobPostRepository;
        _userManager = userManager;
        _logger = logger;
        _context = context;
    }

    public async Task<IEnumerable<JobPost>> GetAllJobPostsAsync()
    {
        return await _jobPostRepository.GetAllAsync();
    }

    public async Task<JobPost?> GetJobPostByIdAsync(int id)
    {
        return await _jobPostRepository.GetByIdAsync(id);
    }
    
    public async Task<IEnumerable<JobPost>> GetJobPostsByCompanyAsync(string companyId)
    {
        return await _jobPostRepository.GetByCompanyIdAsync(companyId);
    }

    
    public async Task<bool> IncrementJobPostViewCountAsync(int jobPostId)
    {
        var jobPost = await _context.JobPosts.FindAsync(jobPostId);
        if (jobPost == null)
        {
            _logger.LogWarning("Attempted to increment view count for non-existent JobPostId: {JobPostId}", jobPostId);
            return false;
        }

        jobPost.ViewCount++;
        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("View count incremented for JobPostId: {JobPostId}. New count: {ViewCount}", jobPostId, jobPost.ViewCount);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error incrementing view count for JobPostId: {JobPostId}", jobPostId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing view count for JobPostId: {JobPostId}", jobPostId);
            return false;
        }
    }
    
    public async Task<(bool Success, string Message)> CreateJobPostAsync(JobPost jobPost, string companyId)
    {
        try
        {
            jobPost.CompanyId = companyId;
            jobPost.PostedDate = DateTime.UtcNow;
            jobPost.IsActive = true;
            
            var createdJobPost = await _jobPostRepository.CreateAsync(jobPost);
            _logger.LogInformation("Job post '{JobTitle}' created successfully for company {CompanyId}.", jobPost.Title, companyId);
            return (true, "Job post created successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job post for company {CompanyId}.", companyId);
            return (false, "An error occurred while creating the job post. Please try again.");
        }
    }
    
    public async Task<(bool Success, string Message)> UpdateJobPostAsync(JobPost jobPost)
    {
        try
        {
            var updatedJobPost = await _jobPostRepository.UpdateAsync(jobPost);
            _logger.LogInformation("Job post '{JobTitle}' ({JobPostId}) updated successfully.", jobPost.Title, jobPost.Id);
            return (true, "Job post updated successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job post {JobPostId}.", jobPost.Id);
            return (false, "An error occurred while updating the job post. Please try again.");
        }
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