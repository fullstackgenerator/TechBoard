using Microsoft.EntityFrameworkCore;
using TechBoard.Data;
using TechBoard.ViewModels.Company.Analytics;

namespace TechBoard.Services;

public class CompanyAnalyticsService : ICompanyAnalyticsService
{
    private readonly ApplicationDbContext _context;

    public CompanyAnalyticsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CompanyAnalyticsViewModel> GetCompanyAnalyticsAsync(string companyId)
    {
        var allCompanyJobPosts = await _context.JobPosts
                                            .Where(jp => jp.CompanyId == companyId)
                                            .Include(jp => jp.JobApplications)
                                            .ToListAsync();

        var viewModel = new CompanyAnalyticsViewModel
        {
            TotalJobPosts = allCompanyJobPosts.Count,
            TotalActiveJobPosts = allCompanyJobPosts.Count(jp => jp.IsActive),
            TotalApplicationsReceived = allCompanyJobPosts.Sum(jp => jp.JobApplications.Count),
            TotalJobPostViews = allCompanyJobPosts.Sum(jp => jp.ViewCount),
            
            AverageApplicationsPerPost = allCompanyJobPosts.Count > 0 
                ? (double)allCompanyJobPosts.Sum(jp => jp.JobApplications.Count) / allCompanyJobPosts.Count 
                : 0,
            AverageViewsPerPost = allCompanyJobPosts.Count > 0 
                ? (double)allCompanyJobPosts.Sum(jp => jp.ViewCount) / allCompanyJobPosts.Count 
                : 0
        };
        
        viewModel.TopPerformingJobPosts = allCompanyJobPosts
            .Select(jp => new CompanyAnalyticsViewModel.JobPostPerformance
            {
                JobPostId = jp.Id,
                Title = jp.Title,
                Applications = jp.JobApplications.Count,
                Views = jp.ViewCount,
                ConversionRate = jp.ViewCount > 0 ? ((double)jp.JobApplications.Count / jp.ViewCount) * 100 : 0
            })
            .OrderByDescending(p => p.Applications)
            .Take(5) // Example: Top 5
            .ToList();

        // Applications by Status
        viewModel.ApplicationsByStatus = allCompanyJobPosts
            .SelectMany(jp => jp.JobApplications)
            .GroupBy(ja => ja.Status.ToString()) // FIXED: Use ToString() for enum value
            .ToDictionary(g => g.Key, g => g.Count());

        // Trend data
        viewModel.ApplicationsByDate = allCompanyJobPosts
            .SelectMany(jp => jp.JobApplications)
            .GroupBy(ja => ja.AppliedDate.Date) // Group by date only
            .Select(g => new CompanyAnalyticsViewModel.ApplicationTrendData
            {
                Date = g.Key,
                Count = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToList();

        return viewModel;
    }
}