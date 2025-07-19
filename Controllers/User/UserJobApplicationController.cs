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
        var userResult = await GetCurrentUserAsync();
        if (!userResult.Success)
            return userResult.Result!;

        var applications = await _jobApplicationService.GetApplicationsByUserIdAsync(userResult.User!.Id);
        var model = MapToJobApplicationViewModels(applications);

        return View(model);
    }

    // GET: user/applications/details/{id}
    [HttpGet("details/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var userResult = await GetCurrentUserAsync();
        if (!userResult.Success)
            return userResult.Result!;

        var application = await _jobApplicationService.GetApplicationDetailsAsync(id);
        if (!IsUserAuthorizedForApplication(application, userResult.User!.Id))
        {
            TempData["ErrorMessage"] = "Application not found or you do not have permission to view it.";
            return NotFound();
        }

        var model = MapToJobApplicationViewModel(application!);
        return View(model);
    }

    // GET: user/applications/apply/{jobPostId}
    [HttpGet("apply/{jobPostId}")]
    public async Task<IActionResult> Apply(int jobPostId)
    {
        var userResult = await GetCurrentUserAsync();
        if (!userResult.Success)
            return userResult.Result!;

        var validationResult = await ValidateJobApplicationEligibilityAsync(jobPostId, userResult.User!.Id);
        if (!validationResult.Success)
            return validationResult.Result!;

        var model = new SubmitJobApplicationViewModel();
        SetJobApplicationViewData(validationResult.JobPost!, userResult.User as Models.Domain.User);

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

        var validationResult = await ValidateJobApplicationEligibilityAsync(jobPostId, userId);
        if (!validationResult.Success)
        {
            if (validationResult.IsAlreadyApplied)
            {
                ModelState.AddModelError(string.Empty, "You have already applied for this job post. One application per post allowed.");
                SetJobApplicationViewData(validationResult.JobPost!, null);
                return View(model);
            }
            return validationResult.Result!;
        }

        if (!ModelState.IsValid)
        {
            SetJobApplicationViewData(validationResult.JobPost!, null);
            return View(model);
        }

        var (success, message) = await _jobApplicationService.ApplyForJobAsync(jobPostId, userId, model);

        if (success)
        {
            TempData["SuccessMessage"] = message;
            return RedirectToAction("AllApplications");
        }

        ModelState.AddModelError(string.Empty, message);
        SetJobApplicationViewData(validationResult.JobPost!, null);
        return View(model);
    }

    #region Private Helper Methods

    private async Task<(bool Success, ApplicationUser? User, IActionResult? Result)> GetCurrentUserAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            var result = RedirectToAction("Login", "Auth");
            return (false, null, result);
        }

        return (true, user, null);
    }

    private async Task<(bool Success, Models.Domain.JobPost? JobPost, bool IsAlreadyApplied, IActionResult? Result)> ValidateJobApplicationEligibilityAsync(
        int jobPostId, string userId)
    {
        var jobPost = await _jobPostService.GetJobPostByIdAsync(jobPostId);
        if (jobPost == null)
        {
            TempData["ErrorMessage"] = "Job post not found.";
            return (false, null, false, NotFound());
        }

        var hasApplied = await _jobApplicationService.HasUserAppliedAsync(userId, jobPostId);
        if (hasApplied)
        {
            TempData["WarningMessage"] = "You have already applied for this job post. One application per post allowed.";
            var result = RedirectToAction("Details", "PublicJobPost", new { id = jobPostId });
            return (false, jobPost, true, result);
        }

        return (true, jobPost, false, null);
    }

    private static bool IsUserAuthorizedForApplication(JobApplication? application, string userId)
    {
        return application != null && application.UserId == userId;
    }

    private static List<JobApplicationViewModel> MapToJobApplicationViewModels(IEnumerable<JobApplication> applications)
    {
        return applications.Select(MapToJobApplicationViewModel).ToList();
    }

    private static JobApplicationViewModel MapToJobApplicationViewModel(JobApplication application)
    {
        return new JobApplicationViewModel
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
    }

    private void SetJobApplicationViewData(Models.Domain.JobPost jobPost, Models.Domain.User? userProfile)
    {
        ViewData["JobTitle"] = jobPost.Title;
        ViewData["JobPostId"] = jobPost.Id;
        
        if (userProfile != null)
        {
            ViewData["UserName"] = $"{userProfile.FirstName} {userProfile.LastName}";
            ViewData["UserEmail"] = userProfile.Email;
            ViewData["UserPhone"] = userProfile.Phone;
            ViewData["UserAddress"] = $"{userProfile.Address}, {userProfile.City}, {userProfile.PostalCode}, {userProfile.Country}";
            ViewData["UserLinkedIn"] = userProfile.LinkedInProfile;
            ViewData["UserGitHub"] = userProfile.GitHubProfile;
            ViewData["UserPortfolio"] = userProfile.Website;
        }
    }

    #endregion
}