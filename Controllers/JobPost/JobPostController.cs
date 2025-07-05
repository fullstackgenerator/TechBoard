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
            _logger.LogWarning(
                "Company user not found or not of type Company for creating a job post. User ID: {UserId}",
                _userManager.GetUserId(User));
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

        if (!await _jobPostService.CanCompanyPostMoreJobsAsync(company.Id))
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
            _logger.LogWarning(
                "Company user not found or not of type Company attempting to view job posts. User ID: {UserId}",
                _userManager.GetUserId(User));
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

    // GET: /company/jobposts/details/{id}
    [HttpGet("jobposts/details/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        // get the current company user to ensure they own the job post
        var companyUser = await _userManager.GetUserAsync(User);
        if (companyUser is not TechBoard.Models.Domain.Company company)
        {
            _logger.LogWarning(
                "Company user not found or not of type Company attempting to view job post details. User ID: {UserId}",
                _userManager.GetUserId(User));
            TempData["ErrorMessage"] = "Could not identify your company profile.";
            return RedirectToAction("Index", "CompanyDashboard");
        }

        // get the job post by ID using the job post service
        var jobPost = await _jobPostService.GetJobPostByIdAsync(id);

        // check if the job post exists and belongs to the current company
        if (jobPost == null || jobPost.CompanyId != company.Id)
        {
            _logger.LogWarning("Job post with ID {JobPostId} not found or does not belong to company {CompanyId}.", id,
                company.Id);
            TempData["ErrorMessage"] = "Job post not found or you do not have permission to view it.";
            return NotFound();
        }

        // map the domain model (JobPost) to the ViewModel (JobPostViewModel)
        var model = new JobPostViewModel
        {
            Id = jobPost.Id,
            Title = jobPost.Title,
            Description = jobPost.Description,
            Location = jobPost.Location,
            PostedDate = jobPost.PostedDate,
            CompanyName = company.Name,
            Requirements = jobPost.Requirements,
            Benefits = jobPost.Benefits,
            SalaryMin = jobPost.SalaryMin,
            SalaryMax = jobPost.SalaryMax,
            Category = jobPost.Category.ToString(),
            JobLevel = jobPost.JobLevel.ToString(),
            WorkType = jobPost.WorkType.ToString(),
            IsRemote = jobPost.IsRemote
        };

        return View(model);
    }

// GET: /company/jobposts/edit/{id}
    [HttpGet("jobposts/edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var companyUser = await _userManager.GetUserAsync(User);
        if (companyUser is not TechBoard.Models.Domain.Company company)
        {
            _logger.LogWarning(
                "Company user not found or not of type Company attempting to edit job post. User ID: {UserId}",
                _userManager.GetUserId(User));
            TempData["ErrorMessage"] = "Could not identify your company profile.";
            return RedirectToAction("Index", "CompanyDashboard");
        }

        var jobPost = await _jobPostService.GetJobPostByIdAsync(id);

        // check if the job post exists and belongs to the current company
        if (jobPost == null || jobPost.CompanyId != company.Id)
        {
            _logger.LogWarning("Job post with ID {JobPostId} not found or does not belong to company {CompanyId}.", id,
                company.Id);
            TempData["ErrorMessage"] = "Job post not found or you do not have permission to edit it.";
            return NotFound();
        }
        
        var model = new EditJobPostViewModel
        {
            Id = jobPost.Id,
            Title = jobPost.Title,
            Description = jobPost.Description,
            Requirements = jobPost.Requirements,
            Location = jobPost.Location,
            IsRemote = jobPost.IsRemote,
            SalaryMin = jobPost.SalaryMin,
            SalaryMax = jobPost.SalaryMax,
            Category = jobPost.Category,
            JobLevel = jobPost.JobLevel,
            WorkType = jobPost.WorkType,
            Benefits = jobPost.Benefits,
            PostedDate = jobPost.PostedDate
        };

        return View(model);
    }

// POST: /company/jobposts/edit/{id}
    [HttpPost("jobposts/edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditJobPostViewModel model)
    {
        if (id != model.Id)
        {
            _logger.LogWarning("Route ID {RouteId} does not match model ID {ModelId} during job post edit.", id,
                model.Id);
            return BadRequest();
        }

        var companyUser = await _userManager.GetUserAsync(User);
        if (companyUser is not TechBoard.Models.Domain.Company company)
        {
            ModelState.AddModelError(string.Empty, "Could not identify company. Please log in again.");
            return View(model);
        }

        // retrieve original job post from the database to update it
        var jobPostToUpdate = await _jobPostService.GetJobPostByIdAsync(id);

        // check ownership again before applying updates
        if (jobPostToUpdate == null || jobPostToUpdate.CompanyId != company.Id)
        {
            _logger.LogWarning(
                "Job post with ID {JobPostId} not found or does not belong to company {CompanyId} during update attempt.",
                id, company.Id);
            TempData["ErrorMessage"] = "Job post not found or you do not have permission to edit it.";
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("EditJobPostViewModel validation failed for job post {JobPostId}", id);
            return View(model);
        }
        
        jobPostToUpdate.Title = model.Title;
        jobPostToUpdate.Description = model.Description;
        jobPostToUpdate.Requirements = model.Requirements;
        jobPostToUpdate.Location = model.Location;
        jobPostToUpdate.IsRemote = model.IsRemote;
        jobPostToUpdate.SalaryMin = model.SalaryMin;
        jobPostToUpdate.SalaryMax = model.SalaryMax;
        jobPostToUpdate.Category = model.Category;
        jobPostToUpdate.JobLevel = model.JobLevel;
        jobPostToUpdate.WorkType = model.WorkType;
        jobPostToUpdate.Benefits = model.Benefits;
        jobPostToUpdate.Updated = DateTime.UtcNow;

        try
        {
            await _jobPostService
                .UpdateJobPostAsync(jobPostToUpdate);
            TempData["SuccessMessage"] = "Job post updated successfully!";
            return RedirectToAction("Details", new { id = jobPostToUpdate.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job post {JobPostId} for company {CompanyId}", id, company.Id);
            ModelState.AddModelError(string.Empty, "An error occurred while updating the job post. Please try again.");
            return View(model);
        }
    }
}