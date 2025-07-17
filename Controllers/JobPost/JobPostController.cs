using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechBoard.Constants;
using TechBoard.Models.Domain;
using TechBoard.Services;
using TechBoard.ViewModels.JobPost;

namespace TechBoard.Controllers.JobPost;

[Authorize(Roles = Roles.Company)]
[Route("company/jobposts")]
public class JobPostsController : Controller
{
    private readonly IJobPostService _jobPostService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMembershipService _membershipService;
    private readonly ILogger<JobPostsController> _logger;

    public JobPostsController(
        IJobPostService jobPostService,
        UserManager<ApplicationUser> userManager,
        IMembershipService membershipService,
        ILogger<JobPostsController> logger)
    {
        _jobPostService = jobPostService;
        _userManager = userManager;
        _membershipService = membershipService;
        _logger = logger;
    }

    // GET: /company/jobposts
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var companyUser = await _userManager.GetUserAsync(User);
        if (companyUser is not Models.Domain.Company company)
        {
            _logger.LogWarning("Non-company user attempted to access company job posts. User ID: {UserId}", _userManager.GetUserId(User));
            TempData["ErrorMessage"] = "Could not identify your company profile.";
            return RedirectToAction("Index", "AdminDashboard");
        }
        
        var jobPosts = await _jobPostService.GetJobPostsByCompanyAsync(company.Id);
        var model = jobPosts.Select(jp => new JobPostViewModel
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

    // GET: /company/jobposts/details/{id}
    [HttpGet("details/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var companyUser = await _userManager.GetUserAsync(User);
        if (companyUser is not Models.Domain.Company company)
        {
            _logger.LogWarning("Non-company user attempted to access job post details. User ID: {UserId}", _userManager.GetUserId(User));
            TempData["ErrorMessage"] = "Could not identify your company profile.";
            return RedirectToAction("Index", "AdminDashboard");
        }
        
        var jobPost = await _jobPostService.GetJobPostByIdAsync(id);
        
        if (jobPost == null || jobPost.CompanyId != company.Id)
        {
            _logger.LogWarning("Job post {JobPostId} not found or not owned by company {CompanyId}. User ID: {UserId}", id, company.Id, _userManager.GetUserId(User));
            TempData["ErrorMessage"] = "Job post not found or you do not have permission to view it.";
            return NotFound();
        }

        var model = new JobPostViewModel
        {
            Id = jobPost.Id,
            Title = jobPost.Title,
            Description = jobPost.Description,
            Location = jobPost.Location,
            PostedDate = jobPost.PostedDate,
            CompanyName = jobPost.Company?.Name ?? "N/A",
            Requirements = jobPost.Requirements,
            Benefits = jobPost.Benefits,
            SalaryMin = jobPost.SalaryMin,
            SalaryMax = jobPost.SalaryMax,
            Category = jobPost.Category.ToString(),
            JobLevel = jobPost.JobLevel.ToString(),
            WorkType = jobPost.WorkType.ToString(),
            IsRemote = jobPost.IsRemote,
            IsFeatured = jobPost.IsFeatured
        };

        return View(model);
    }

    // GET: /company/jobposts/create
    [HttpGet("create")]
    public async Task<IActionResult> Create()
    {
        var companyUser = await _userManager.GetUserAsync(User) as Models.Domain.Company;
        if (companyUser == null)
        {
            _logger.LogWarning("Company profile not found for user attempting to create job post. User ID: {UserId}", _userManager.GetUserId(User));
            TempData["ErrorMessage"] = "Company profile not found.";
            return RedirectToAction("Index", "AdminDashboard");
        }

        var currentTier = await _membershipService.GetMembershipTierByIdAsync(companyUser.MembershipTierId);
        ViewBag.CanPostFeatured = currentTier?.CanPostFeatured ?? false;
        
        return View(new CreateJobPostViewModel()); 
    }

    // POST: /company/jobposts/create
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateJobPostViewModel model)
    {
        var companyUser = await _userManager.GetUserAsync(User) as Models.Domain.Company;
        if (companyUser == null)
        {
            _logger.LogWarning("Company profile not found for user attempting to create job post (POST). User ID: {UserId}", _userManager.GetUserId(User));
            TempData["ErrorMessage"] = "Company profile not found.";
            return RedirectToAction("Index", "AdminDashboard");
        }
        
        var currentTier = await _membershipService.GetMembershipTierByIdAsync(companyUser.MembershipTierId);
        ViewBag.CanPostFeatured = currentTier?.CanPostFeatured ?? false;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.IsFeatured && !(currentTier?.CanPostFeatured ?? false))
        {
            ModelState.AddModelError(string.Empty, "Your current membership tier does not allow featured job posts. This job will be posted as a regular job.");
            model.IsFeatured = false;
        }

        var jobPost = new Models.Domain.JobPost
        {
            Title = model.Title,
            Description = model.Description,
            Requirements = model.Requirements,
            Location = model.Location,
            IsRemote = model.IsRemote,
            SalaryMin = model.SalaryMin,
            SalaryMax = model.SalaryMax,
            Category = model.Category,
            JobLevel = model.JobLevel,
            WorkType = model.WorkType,
            Benefits = model.Benefits,
            CompanyId = companyUser.Id,
            IsFeatured = model.IsFeatured
        };
        
        var (success, message) = await _jobPostService.CreateJobPostAsync(jobPost, companyUser.Id);

        if (success)
        {
            TempData["SuccessMessage"] = message;
            _logger.LogInformation("Job post '{JobTitle}' created successfully for company {CompanyId}.", jobPost.Title, companyUser.Id);
            return RedirectToAction("Index");
        }
        else
        {
            ModelState.AddModelError(string.Empty, message);
            _logger.LogWarning("Failed to create job post for company {CompanyId}: {Message}", companyUser.Id, message);
            return View(model);
        }
    }
    
    // GET: /company/jobposts/edit/{id}
    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var companyUser = await _userManager.GetUserAsync(User) as Models.Domain.Company;
        if (companyUser == null)
        {
            _logger.LogWarning("Company profile not found for user attempting to edit job post. User ID: {UserId}", _userManager.GetUserId(User));
            TempData["ErrorMessage"] = "Company profile not found.";
            return RedirectToAction("Index", "AdminDashboard");
        }

        var jobPost = await _jobPostService.GetJobPostByIdAsync(id);
        if (jobPost == null || jobPost.CompanyId != companyUser.Id)
        {
            _logger.LogWarning("Job post {JobPostId} not found or not owned by company {CompanyId}. User ID: {UserId}", id, companyUser.Id, _userManager.GetUserId(User));
            TempData["ErrorMessage"] = "Job post not found or you do not have permission to edit it.";
            return NotFound();
        }

        var currentTier = await _membershipService.GetMembershipTierByIdAsync(companyUser.MembershipTierId);
        ViewBag.CanPostFeatured = currentTier?.CanPostFeatured ?? false;

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
            PostedDate = jobPost.PostedDate,
            IsFeatured = jobPost.IsFeatured
        };

        return View(model);
    }

    // POST: /company/jobposts/edit/{id}
    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditJobPostViewModel model)
    {
        if (id != model.Id)
        {
            _logger.LogWarning("Mismatched ID in job post edit (route ID: {RouteId}, model ID: {ModelId}).", id, model.Id);
            return NotFound();
        }

        var companyUser = await _userManager.GetUserAsync(User) as Models.Domain.Company;
        if (companyUser == null)
        {
            _logger.LogWarning("Company profile not found for user attempting to edit job post (POST). User ID: {UserId}", _userManager.GetUserId(User));
            TempData["ErrorMessage"] = "Company profile not found.";
            return RedirectToAction("Index", "AdminDashboard");
        }
        
        var currentTier = await _membershipService.GetMembershipTierByIdAsync(companyUser.MembershipTierId);
        ViewBag.CanPostFeatured = currentTier?.CanPostFeatured ?? false;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existingJobPost = await _jobPostService.GetJobPostByIdAsync(id);
        if (existingJobPost == null || existingJobPost.CompanyId != companyUser.Id)
        {
            _logger.LogWarning("Job post {JobPostId} not found or not owned by company {CompanyId} during edit. User ID: {UserId}", id, companyUser.Id, _userManager.GetUserId(User));
            TempData["ErrorMessage"] = "Job post not found or you do not have permission to edit it.";
            return NotFound();
        }

        if (model.IsFeatured && !(currentTier?.CanPostFeatured ?? false))
        {
            ModelState.AddModelError(string.Empty, "Your current membership tier does not allow featured job posts. This job will be saved as a regular job.");
            model.IsFeatured = false;
        }
        
        existingJobPost.Title = model.Title;
        existingJobPost.Description = model.Description;
        existingJobPost.Requirements = model.Requirements;
        existingJobPost.Location = model.Location;
        existingJobPost.IsRemote = model.IsRemote;
        existingJobPost.SalaryMin = model.SalaryMin;
        existingJobPost.SalaryMax = model.SalaryMax;
        existingJobPost.Category = model.Category;
        existingJobPost.JobLevel = model.JobLevel;
        existingJobPost.WorkType = model.WorkType;
        existingJobPost.Benefits = model.Benefits;
        existingJobPost.IsFeatured = model.IsFeatured;
        
        var (success, message) = await _jobPostService.UpdateJobPostAsync(existingJobPost);

        if (success)
        {
            TempData["SuccessMessage"] = message;
            _logger.LogInformation("Job post '{JobTitle}' ({JobPostId}) updated successfully by company {CompanyId}.", existingJobPost.Title, existingJobPost.Id, companyUser.Id);
            return RedirectToAction("Index");
        }
        else
        {
            ModelState.AddModelError(string.Empty, message);
            _logger.LogWarning("Failed to update job post {JobPostId} for company {CompanyId}: {Message}", existingJobPost.Id, companyUser.Id, message);
            return View(model);
        }
    }

    // POST: /company/jobposts/delete/{id}
    [HttpPost("delete/{id}")] 
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var companyUser = await _userManager.GetUserAsync(User);
        if (companyUser is not TechBoard.Models.Domain.Company company)
        {
            _logger.LogWarning(
                "Company user not found or not of type Company attempting to delete job post. User ID: {UserId}",
                _userManager.GetUserId(User));
            TempData["ErrorMessage"] = "Could not identify your company profile.";
            return RedirectToAction("Index", "AdminDashboard");
        }

        try
        {
            var isDeleted = await _jobPostService.DeleteJobPostAsync(id, company.Id);

            if (!isDeleted)
            {
                _logger.LogWarning(
                    "Failed to delete job post {JobPostId} for company {CompanyId}. It might not exist or not belong to this company.",
                    id, company.Id);
                TempData["ErrorMessage"] = "Job post not found or you do not have permission to delete it.";
                return NotFound();
            }

            TempData["SuccessMessage"] = "Job post deleted successfully!";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job post {JobPostId} for company {CompanyId}", id, company.Id);
            TempData["ErrorMessage"] = "An error occurred while deleting the job post. Please try again.";
            return RedirectToAction("Index");
        }
    }
}