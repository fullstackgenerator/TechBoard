using Microsoft.EntityFrameworkCore;
using TechBoard.Data;
using TechBoard.Models.Domain;

namespace TechBoard.Repositories;

public class JobPostRepository : IJobPostRepository
{
    private readonly ApplicationDbContext _context;

    public JobPostRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<JobPost>> GetAllAsync()
    {
        return await _context.JobPosts
            .Include(j => j.Company)
            .Where(j => j.IsActive)
            .OrderByDescending(j => j.PostedDate)
            .ToListAsync();
    }

    public async Task<JobPost?> GetByIdAsync(int id)
    {
        return await _context.JobPosts
            .Include(j => j.Company)
            .FirstOrDefaultAsync(j => j.Id == id);
    }

    public async Task<IEnumerable<JobPost>> GetByCompanyIdAsync(string companyId)
    {
        return await _context.JobPosts
            .Include(j => j.Company)
            .Where(j => j.CompanyId == companyId)
            .OrderByDescending(j => j.PostedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<JobPost>> GetActiveByCategoryAsync(Category category)
    {
        return await _context.JobPosts
            .Include(j => j.Company)
            .Where(j => j.Category == category && j.IsActive)
            .OrderByDescending(j => j.PostedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<JobPost>> SearchAsync(string searchTerm, Category? category = null, string? location = null)
    {
        var query = _context.JobPosts
            .Include(j => j.Company)
            .Where(j => j.IsActive);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(j => j.Title.Contains(searchTerm) || 
                                   j.Description.Contains(searchTerm) ||
                                   j.Company.Name.Contains(searchTerm));
        }

        if (category.HasValue)
        {
            query = query.Where(j => j.Category == category.Value);
        }

        if (!string.IsNullOrEmpty(location))
        {
            query = query.Where(j => j.Location.Contains(location));
        }

        return await query.OrderByDescending(j => j.PostedDate).ToListAsync();
    }

    public async Task<JobPost> CreateAsync(JobPost jobPost)
    {
        _context.JobPosts.Add(jobPost);
        await _context.SaveChangesAsync();
        return jobPost;
    }

    public async Task<JobPost> UpdateAsync(JobPost jobPost)
    {
        jobPost.Updated = DateTime.UtcNow;
        _context.JobPosts.Update(jobPost);
        await _context.SaveChangesAsync();
        return jobPost;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var jobPost = await _context.JobPosts.FindAsync(id);
        if (jobPost == null) return false;

        _context.JobPosts.Remove(jobPost);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetActiveCountByCompanyAsync(string companyId)
    {
        return await _context.JobPosts
            .CountAsync(j => j.CompanyId == companyId && j.IsActive);
    }

    public async Task IncrementViewCountAsync(int jobPostId)
    {
        var jobPost = await _context.JobPosts.FindAsync(jobPostId);
        if (jobPost != null)
        {
            jobPost.ViewCount++;
            await _context.SaveChangesAsync();
        }
    }
}