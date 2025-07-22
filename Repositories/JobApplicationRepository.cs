using Microsoft.EntityFrameworkCore;
using TechBoard.Data;
using TechBoard.Models.Domain;

namespace TechBoard.Repositories
{
    public class JobApplicationRepository : IJobApplicationRepository
    {
        private readonly ApplicationDbContext _context;

        public JobApplicationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<JobApplication> CreateAsync(JobApplication application)
        {
            await _context.JobApplications.AddAsync(application);
            await _context.SaveChangesAsync();
            return application;
        }

        public async Task<IEnumerable<JobApplication>> GetByUserIdAsync(string userId)
        {
            return await _context.JobApplications
                .Where(a => a.UserId == userId)
                .Include(a => a.JobPost)
                .ThenInclude(jp => jp.Company)
                .ToListAsync();
        }

        public async Task<JobApplication?> GetByIdAsync(int applicationId)
        {
            return await _context.JobApplications
                .Include(a => a.User)
                .Include(a => a.JobPost)
                .ThenInclude(jp => jp.Company)
                .FirstOrDefaultAsync(a => a.Id == applicationId);
        }
        
        public async Task<IEnumerable<JobApplication>> GetByJobPostIdAsync(int jobPostId)
        {
            return await _context.JobApplications
                .Where(a => a.JobPostId == jobPostId)
                .Include(a => a.User)
                .OrderByDescending(a => a.AppliedDate)
                .ToListAsync();
        }

        public async Task<JobApplication> UpdateAsync(JobApplication application)
        {
            _context.JobApplications.Update(application);
            await _context.SaveChangesAsync();
            return application;
        }
    }
}