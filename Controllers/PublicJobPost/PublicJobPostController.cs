using Microsoft.AspNetCore.Mvc;
using TechBoard.Services;
using TechBoard.ViewModels.PublicJobPost;

namespace TechBoard.Controllers.PublicJobPost;

[Route("jobs")]
public class PublicJobPostController : Controller
{
    public readonly IJobPostService JobPostService;
    public readonly ILogger<PublicJobPostController> Logger;

    public PublicJobPostController(IJobPostService jobPostService, ILogger<PublicJobPostController> logger)
    {
        JobPostService = jobPostService;
        Logger = logger;
    }

    // GET: /jobs
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var jobPosts = await JobPostService.GetAllJobPostsAsync();
        var model = jobPosts.Select(jp => new PublicJobPostViewModel
        {
            Id = jp.Id,
            Title = jp.Title,
            Description = jp.Description,
            Location = jp.Location,
            PostedDate = jp.PostedDate,
            CompanyName = jp.Company?.Name ?? "N/A",
            Requirements = jp.Requirements,
            Benefits = jp.Benefits,
            SalaryMin = jp.SalaryMin,
            SalaryMax = jp.SalaryMax,
            Category = jp.Category.ToString(),
            JobLevel = jp.JobLevel.ToString(),
            WorkType = jp.WorkType.ToString(),
            IsRemote = jp.IsRemote,
            IsFeatured = jp.IsFeatured
        }).ToList();
        
        return View(model);
    }

    // GET: /jobs/details/{id}
    [HttpGet("details/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var publicjobPost = await JobPostService.GetJobPostByIdAsync(id);

        if (publicjobPost == null)
        {
            Logger.LogWarning("Job post with ID {JobPostId} not found for public viewing.", id);
            return NotFound();
        }
        
        var model = new PublicJobPostViewModel
        {
            Id = publicjobPost.Id,
            Title = publicjobPost.Title,
            Description = publicjobPost.Description,
            Location = publicjobPost.Location,
            PostedDate = publicjobPost.PostedDate,
            CompanyName = publicjobPost.Company?.Name ?? "N/A",
            Requirements = publicjobPost.Requirements,
            Benefits = publicjobPost.Benefits,
            SalaryMin = publicjobPost.SalaryMin,
            SalaryMax = publicjobPost.SalaryMax,
            Category = publicjobPost.Category.ToString(),
            JobLevel = publicjobPost.JobLevel.ToString(),
            WorkType = publicjobPost.WorkType.ToString(),
            IsRemote = publicjobPost.IsRemote,
            IsFeatured = publicjobPost.IsFeatured
        };
        
        return View(model);
    }
}