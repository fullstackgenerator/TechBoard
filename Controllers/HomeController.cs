using Microsoft.AspNetCore.Mvc;
using TechBoard.Models.Domain;
using TechBoard.Services;
using TechBoard.ViewModels.PublicJobPost;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace TechBoard.Controllers;

public class HomeController : Controller
{
    public readonly IJobPostService JobPostService;
    public readonly ILogger<HomeController> Logger;

    public HomeController(IJobPostService jobPostService, ILogger<HomeController> logger)
    {
        JobPostService = jobPostService;
        Logger = logger;
    }

    // get the display name of an enum value
    private static string GetEnumDisplayName(Enum enumValue)
    {
        return enumValue.GetType()
            .GetMember(enumValue.ToString())
            .FirstOrDefault()?
            .GetCustomAttribute<DisplayAttribute>()?
            .GetName() ?? enumValue.ToString();
    }

    // get enum value from display name
    private static T? GetEnumFromDisplayName<T>(string displayName) where T : struct, Enum
    {
        foreach (T enumValue in Enum.GetValues<T>())
        {
            if (GetEnumDisplayName(enumValue).Equals(displayName, StringComparison.OrdinalIgnoreCase))
            {
                return enumValue;
            }
        }
        return null;
    }

    // GET: /jobs or /
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] PublicJobPostViewModel.JobPostFilterViewModel filters)
    {

        var allJobPosts = (await JobPostService.GetAllJobPostsAsync()).ToList();
        
        var filteredJobPosts = allJobPosts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            var searchTermLower = filters.SearchTerm.ToLower();
            filteredJobPosts = filteredJobPosts.Where(jp =>
                jp.Title.ToLower().Contains(searchTermLower) ||
                jp.Company.Name.ToLower().Contains(searchTermLower) ||
                jp.Description.ToLower().Contains(searchTermLower));
        }

        if (!string.IsNullOrWhiteSpace(filters.Location))
        {
            filteredJobPosts = filteredJobPosts.Where(jp => jp.Location.ToLower() == filters.Location.ToLower());
        }
        
        if (!string.IsNullOrWhiteSpace(filters.Category))
        {
            var categoryEnum = GetEnumFromDisplayName<Category>(filters.Category);
            if (categoryEnum.HasValue)
            {
                filteredJobPosts = filteredJobPosts.Where(jp => jp.Category == categoryEnum.Value);
            }
        }
        
        if (!string.IsNullOrWhiteSpace(filters.JobLevel))
        {
            var jobLevelEnum = GetEnumFromDisplayName<JobLevel>(filters.JobLevel);
            if (jobLevelEnum.HasValue)
            {
                filteredJobPosts = filteredJobPosts.Where(jp => jp.JobLevel == jobLevelEnum.Value);
            }
        }
        
        if (!string.IsNullOrWhiteSpace(filters.WorkType))
        {
            var workTypeEnum = GetEnumFromDisplayName<WorkType>(filters.WorkType);
            if (workTypeEnum.HasValue)
            {
                filteredJobPosts = filteredJobPosts.Where(jp => jp.WorkType == workTypeEnum.Value);
            }
        }

        if (filters.IsRemote.HasValue)
        {
            filteredJobPosts = filteredJobPosts.Where(jp => jp.IsRemote == filters.IsRemote.Value);
        }

        if (filters.SalaryMin.HasValue)
        {
            filteredJobPosts = filteredJobPosts.Where(jp => jp.SalaryMax >= filters.SalaryMin.Value);
        }

        if (filters.SalaryMax.HasValue)
        {
            filteredJobPosts = filteredJobPosts.Where(jp => jp.SalaryMin <= filters.SalaryMax.Value);
        }

        var sortedJobPosts = filteredJobPosts
            .OrderByDescending(jp => jp.IsFeatured)
            .ThenByDescending(jp => jp.PostedDate)
            .ToList();

        var jobPostViewModels = sortedJobPosts.Select(jp => new PublicJobPostViewModel
        {
            Id = jp.Id,
            Title = jp.Title,
            Description = jp.Description,
            Location = jp.Location,
            PostedDate = jp.PostedDate,
            CompanyName = jp.Company.Name,
            Requirements = jp.Requirements,
            Benefits = jp.Benefits,
            SalaryMin = jp.SalaryMin,
            SalaryMax = jp.SalaryMax,
            Category = GetEnumDisplayName(jp.Category),
            JobLevel = GetEnumDisplayName(jp.JobLevel),
            WorkType = GetEnumDisplayName(jp.WorkType),
            IsRemote = jp.IsRemote,
            IsFeatured = jp.IsFeatured
        }).ToList();
        
        var availableCategories = Enum.GetValues(typeof(Category))
                                    .Cast<Category>()
                                    .Select(c => GetEnumDisplayName(c))
                                    .ToList();

        var availableJobLevels = Enum.GetValues(typeof(JobLevel))
                                   .Cast<JobLevel>()
                                   .Select(jl => GetEnumDisplayName(jl))
                                   .ToList();

        var availableWorkTypes = Enum.GetValues(typeof(WorkType))
                                   .Cast<WorkType>()
                                   .Select(wt => GetEnumDisplayName(wt))
                                   .ToList();

        var availableLocations = allJobPosts.Select(jp => jp.Location).Distinct().OrderBy(l => l).ToList();

        var model = new JobPostsListViewModel
        {
            JobPosts = jobPostViewModels,
            Filters = filters,
            AvailableCategories = availableCategories,
            AvailableJobLevels = availableJobLevels,
            AvailableWorkTypes = availableWorkTypes,
            AvailableLocations = availableLocations
        };

        return View(model);
    }
}