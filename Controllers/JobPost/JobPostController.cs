using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechBoard.Constants;
using TechBoard.Models.Domain;
using TechBoard.Services;
using TechBoard.ViewModels.JobPost;

namespace TechBoard.Controllers.JobPost;

[Authorize(Roles = Roles.Company)]
[Route("company")]
public class JobPostsController : Controller
{
    private readonly ILogger<JobPostsController> _logger;
    private readonly IJobPostService _jobPostService;
    private readonly UserManager<ApplicationUser> _userManager;

    public JobPostsController(
        IJobPostService jobPostService,
        UserManager<ApplicationUser> userManager,
        ILogger<JobPostsController> logger)
    {
        _jobPostService = jobPostService;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: /company/new-post
    [HttpGet("create")]
    public async Task<IActionResult> Create()
    {
        var companyUser = await _userManager.GetUserAsync(User);
        if (companyUser is not TechBoard.Models.Domain.Company company)
        {
            _logger.LogWarning("Company user not found or not of type Company for creating a job post. User ID: {UserId}", _userManager.GetUserId(User));
            TempData["ErrorMessage"] = "Could not identify your company profile.";
            return RedirectToAction("Index", "CompanyDashboard");
        }
        
        if (!await _jobPostService.CanCompanyPostMoreJobsAsync(company.Id))
        {
            TempData["ErrorMessage"] = "You have reached your maximum job post limit based on your membership tier.";
            return RedirectToAction("Index", "CompanyDashboard");
        }
        
        return View();
    }
    
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateJobPostViewModel model)
    {
        var companyUser = await _userManager.GetUserAsync(User);
        if (companyUser is not TechBoard.Models.Domain.Company company)
        {
            ModelState.AddModelError(string.Empty, "Could not identify company. Please log in again.");
            return View(model);
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("CreateJobPostViewModel validation failed for company {CompanyId}", company.Id);
            return View(model);
        }

        if (!await _jobPostService.CanCompanyPostMoreJobsAsync(company.Id)) // Use company.Id
        {
            TempData["ErrorMessage"] = "You have reached your maximum job post limit based on your membership tier.";
            return RedirectToAction("Index", "CompanyDashboard");
        }

        var jobPost = new Models.Domain.JobPost
        {
            Title = model.Title,
            Description = model.Description,
            Location = model.Location,
            Requirements = model.Requirements,
            IsRemote = model.IsRemote,
            SalaryMin = model.SalaryMin,
            SalaryMax = model.SalaryMax,
            Currency = model.Currency,
            Category = model.Category,
            JobLevel = model.JobLevel,
            WorkType = model.WorkType,
            Benefits = model.Benefits,
            
            PostedDate = DateTime.UtcNow,
            IsActive = true,
            ViewCount = 0,
            CompanyId = company.Id
        };

        try
        {
            var createdJobPost = await _jobPostService.CreateJobPostAsync(jobPost, company.Id);
            TempData["SuccessMessage"] = "Job post created successfully!";
            return RedirectToAction("Index", "JobPosts");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job post for company {CompanyId}", company.Id);
            ModelState.AddModelError(string.Empty, "An error occurred while creating the job post. Please try again.");
            return View(model);
        }
    }
    
    // GET: /company/jobposts
    [HttpGet("jobposts")]
    public async Task<IActionResult> Index()
    {
        var companyUser = await _userManager.GetUserAsync(User);
        if (companyUser is not TechBoard.Models.Domain.Company company)
        {
            _logger.LogWarning("Company user not found or not of type Company attempting to view job posts. User ID: {UserId}", _userManager.GetUserId(User));
            TempData["ErrorMessage"] = "Could not identify your company profile.";
            return RedirectToAction("Index", "CompanyDashboard");
        }

        var jobPosts = await _jobPostService.GetJobPostsByCompanyAsync(company.Id);

        var companyName = company.Name; 

        var model = jobPosts.Select(jp => new JobPostViewModel
        {
            Id = jp.Id,
            Title = jp.Title,
            Description = jp.Description,
            Location = jp.Location,
            PostedDate = jp.PostedDate,
            CompanyName = companyName
        }).ToList();

        return View(model);
    }
}