namespace TechBoard.ViewModels.Company.Analytics;

public class CompanyAnalyticsViewModel
{
    public int TotalJobPosts { get; set; }
    public int TotalActiveJobPosts { get; set; }
    public int TotalApplicationsReceived { get; set; }
    public int TotalJobPostViews { get; set; }
    public double AverageApplicationsPerPost { get; set; }
    public double AverageViewsPerPost { get; set; }
    
    public List<ApplicationTrendData>? ApplicationsByDate { get; set; }
    public List<ViewTrendData>? ViewsByDate { get; set; }
    public List<JobPostPerformance>? TopPerformingJobPosts { get; set; }
    public Dictionary<string, int>? ApplicationsByStatus { get; set; }
    
    public class ApplicationTrendData
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    public class ViewTrendData
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    public class JobPostPerformance
    {
        public int JobPostId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Applications { get; set; }
        public int Views { get; set; }
        public double ConversionRate { get; set; }
    }
}