using Microsoft.EntityFrameworkCore;
using TechBoard.Data;
using TechBoard.Models.Domain;

namespace TechBoard.Repositories;

public class JobApplicationRepository : IJobApplicationRepository
{
    private readonly ApplicationDbContext _context;

    public JobApplicationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<JobApplication>> GetByJobPostIdAsync(int jobPostId)
    {
        return await _context.JobApplications
            .Include(a => a.User)
            .Include(a => a.JobPost)
            .Where(a => a.JobPostId == jobPostId)
            .OrderByDescending(a => a.AppliedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<JobApplication>> GetByUserIdAsync(string userId)
    {
        return await _context.JobApplications
            .Include(a => a.JobPost)
            .ThenInclude(j => j.Company)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.AppliedDate)
            .ToListAsync();
    }

    public async Task<JobApplication?> GetByIdAsync(int id)
    {
        return await _context.JobApplications
            .Include(a => a.User)
            .Include(a => a.JobPost)
            .ThenInclude(j => j.Company)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<JobApplication> CreateAsync(JobApplication application)
    {
        _context.JobApplications.Add(application);
        await _context.SaveChangesAsync();
        return application;
    }

    public async Task<JobApplication> UpdateAsync(JobApplication application)
    {
        application.Updated = DateTime.UtcNow;
        _context.JobApplications.Update(application);
        await _context.SaveChangesAsync();
        return application;
    }

    public async Task<bool> HasUserAppliedAsync(string userId, int jobPostId)
    {
        return await _context.JobApplications
            .AnyAsync(a => a.UserId == userId && a.JobPostId == jobPostId);
    }
}