using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechBoard.Constants;
using TechBoard.Models.Domain;
using TechBoard.Services;
using TechBoard.ViewModels.JobApplication;
using TechBoard.ViewModels.User.Profile;

namespace TechBoard.Controllers.User;

[Authorize(Roles = Roles.User)]
[Route("user/applications")]
public class UserJobApplicationController : Controller
{
    private readonly IJobApplicationService _jobApplicationService;
    private readonly IJobPostService _jobPostService;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserJobApplicationController(
        IJobApplicationService jobApplicationService,
        IJobPostService jobPostService,
        UserManager<ApplicationUser> userManager)
    {
        _jobApplicationService = jobApplicationService;
        _jobPostService = jobPostService;
        _userManager = userManager;
    }

    // GET: user/applications/all
    [HttpGet("all")]
    public async Task<IActionResult> AllApplications()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("Login", "UserAuth");
        }

        var applications = await _jobApplicationService.GetApplicationsByUserIdAsync(user.Id);

        var model = applications.Select(a => new JobApplicationViewModel
        {
            Id = a.Id,
            JobTitle = a.JobPost.Title,
            CompanyName = a.JobPost.Company.Name,
            CoverLetter = a.CoverLetterFileName,
            CvFileName = a.ResumeFileName,
            ResumeFileName = a.ResumeFileName,
            AppliedDate = a.AppliedDate,
            Status = a.Status.ToString(),
            CompanyNotes = a.CompanyNotes,
            Notes = a.ApplicantNotes,
            JobPostId = a.JobPostId
        }).ToList();

        return View(model);
    }

    // GET: user/job-applications/details/{id}
    [HttpGet("details/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("Login", "UserAuth");
        }

        var application = await _jobApplicationService.GetApplicationDetailsAsync(id);

        if (application == null || application.UserId != user.Id)
        {
            TempData["ErrorMessage"] = "Application not found or you do not have permission to view it.";
            return NotFound();
        }

        var model = new JobApplicationViewModel
        {
            Id = application.Id,
            JobTitle = application.JobPost.Title,
            CompanyName = application.JobPost.Company.Name,
            CoverLetter = application.CoverLetterFileName,
            CvFileName = application.ResumeFileName,
            ResumeFileName = application.ResumeFileName,
            AppliedDate = application.AppliedDate,
            Status = application.Status.ToString(),
            CompanyNotes = application.CompanyNotes,
            Notes = application.ApplicantNotes,
            JobPostId = application.JobPostId
        };

        return View(model);
    }

    // GET: user/job-applications/apply/{jobPostId}
    [HttpGet("apply/{jobPostId}")]
    public async Task<IActionResult> Apply(int jobPostId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Please log in to apply for jobs.";
            return RedirectToAction("Login", "UserAuth");
        }

        var jobPost = await _jobPostService.GetJobPostByIdAsync(jobPostId);
        if (jobPost == null)
        {
            TempData["ErrorMessage"] = "Job post not found.";
            return NotFound();
        }
        
        var hasApplied = await _jobApplicationService.HasUserAppliedAsync(user.Id, jobPostId);
        if (hasApplied)
        {
            TempData["WarningMessage"] = "You have already applied for this job post. One application per post allowed.";
            return RedirectToAction("Details", "PublicJobPost", new { id = jobPostId });
        }
        
        var userProfile = user as Models.Domain.User;
        var model = new SubmitJobApplicationViewModel
        {
        };

        ViewData["JobTitle"] = jobPost.Title;
        ViewData["JobPostId"] = jobPostId;
        ViewData["UserName"] = $"{userProfile?.FirstName} {userProfile?.LastName}";
        ViewData["UserEmail"] = userProfile?.Email;
        ViewData["UserPhone"] = userProfile?.Phone;
        ViewData["UserAddress"] = $"{userProfile?.Address}, {userProfile?.City}, {userProfile?.PostalCode}, {userProfile?.Country}";
        ViewData["UserLinkedIn"] = userProfile?.LinkedInProfile;
        ViewData["UserGitHub"] = userProfile?.GitHubProfile;
        ViewData["UserPortfolio"] = userProfile?.Website;
        
        return View(model);
    }

    // POST: user/applications/apply/{jobPostId}
    [HttpPost("apply/{jobPostId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(int jobPostId, SubmitJobApplicationViewModel model)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            ModelState.AddModelError(string.Empty, "User not found. Please log in again.");
            return View(model);
        }

        var jobPost = await _jobPostService.GetJobPostByIdAsync(jobPostId);
        if (jobPost == null)
        {
            TempData["ErrorMessage"] = "Job post not found.";
            return NotFound();
        }
        
        var hasApplied = await _jobApplicationService.HasUserAppliedAsync(userId, jobPostId);
        if (hasApplied)
        {
            TempData["WarningMessage"] = "You have already applied for this job post. One application per post allowed.";
            ModelState.AddModelError(string.Empty, "You have already applied for this job post. One application per post allowed.");
            ViewData["JobTitle"] = jobPost.Title;
            ViewData["JobPostId"] = jobPostId;
            return View(model);
        }

        if (!ModelState.IsValid)
        {
            ViewData["JobTitle"] = jobPost.Title;
            ViewData["JobPostId"] = jobPostId;
            return View(model);
        }

        var (success, message) = await _jobApplicationService.ApplyForJobAsync(jobPostId, userId, model);

        if (success)
        {
            TempData["SuccessMessage"] = message;
            return RedirectToAction("AllApplications");
        }
        else
        {
            ModelState.AddModelError(string.Empty, message);
            ViewData["JobTitle"] = jobPost.Title;
            ViewData["JobPostId"] = jobPostId;
            return View(model);
        }
    }
}