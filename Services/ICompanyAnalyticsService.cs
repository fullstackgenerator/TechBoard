using TechBoard.ViewModels.Company.Analytics;

namespace TechBoard.Services;

public interface ICompanyAnalyticsService
{
    Task<CompanyAnalyticsViewModel> GetCompanyAnalyticsAsync(string companyId);
}