using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechBoard.Constants;
using TechBoard.Models.Domain;
using TechBoard.Services;
using TechBoard.ViewModels.Company.JobApplication;

namespace TechBoard.Controllers.Company;

[Authorize(Roles = Roles.Company)]
[Route("company/jobposts")]
public class CompanyJobApplicationController : Controller
{
    private readonly IJobApplicationService _jobApplicationService;
    private readonly IJobPostService _jobPostService;
    private readonly UserManager<ApplicationUser> _userManager;

    public CompanyJobApplicationController(
        IJobApplicationService jobApplicationService,
        IJobPostService jobPostService,
        UserManager<ApplicationUser> userManager)
    {
        _jobApplicationService = jobApplicationService;
        _jobPostService = jobPostService;
        _userManager = userManager;
    }

    // GET: /company/jobposts/{jobPostId}/applications
    [HttpGet("{jobPostId}/applications")]
    public async Task<IActionResult> Applications(int jobPostId)
    {
        var companyResult = await GetCurrentCompanyAsync();
        if (!companyResult.Success)
            return companyResult.Result!;

        var jobPost = await ValidateJobPostAccessAsync(jobPostId, companyResult.Company!.Id);
        if (jobPost == null)
        {
            TempData["ErrorMessage"] = "Job post not found or you do not have permission to view its applications.";
            return NotFound();
        }

        var applications = await _jobApplicationService.GetApplicationsByCompanyJobPostAsync(companyResult.Company.Id, jobPostId);
        var model = MapToCompanyJobApplicationViewModels(applications, jobPost);

        ViewData["JobPostTitle"] = jobPost.Title;
        ViewData["JobPostId"] = jobPostId;

        return View(model);
    }

    // GET: /company/jobposts/applications/{applicationId}/details
    [HttpGet("applications/{applicationId}/details")]
    public async Task<IActionResult> ApplicationDetails(int applicationId)
    {
        var companyResult = await GetCurrentCompanyAsync();
        if (!companyResult.Success)
            return companyResult.Result!;

        var application = await _jobApplicationService.GetCompanyJobApplicationDetailsAsync(applicationId, companyResult.Company!.Id);
        if (application == null)
        {
            TempData["ErrorMessage"] = "Application not found or you do not have permission to view it.";
            return NotFound();
        }
        
        var model = MapToCompanyJobApplicationViewModel(application);
        ViewBag.StatusList = CreateStatusSelectList(application.Status);
        ViewData["JobPostId"] = application.JobPostId;

        return View(model);
    }

    // POST: /company/jobposts/applications/{applicationId}/update-status
    [HttpPost("applications/{applicationId}/update-status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateApplicationStatus(int applicationId, [FromForm] ApplicationStatus newStatus)
    {
        var companyResult = await GetCurrentCompanyAsync();
        if (!companyResult.Success)
            return companyResult.Result!;

        var (success, message) = await _jobApplicationService.UpdateJobApplicationStatusAsync(
            applicationId, companyResult.Company!.Id, newStatus);

        SetTempDataMessage(success, message);
        return RedirectToAction("ApplicationDetails", new { applicationId });
    }
    
    // POST: /company/jobposts/applications/{applicationId}/update-notes
    [HttpPost("applications/{applicationId}/update-notes")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCompanyNotes(int applicationId, [FromForm] string companyNotes)
    {
        var companyResult = await GetCurrentCompanyAsync();
        if (!companyResult.Success)
            return companyResult.Result!;

        var (success, message) = await _jobApplicationService.UpdateCompanyNotesAsync(
            applicationId, companyResult.Company!.Id, companyNotes);

        SetTempDataMessage(success, message);
        return RedirectToAction("ApplicationDetails", new { applicationId });
    }
    
    // GET: /company/jobposts/applications/download/{applicationId}/{fileType}
    [HttpGet("applications/download/{applicationId}/{fileType}")]
    public async Task<IActionResult> DownloadApplicationFile(int applicationId, string fileType)
    {
        var companyResult = await GetCurrentCompanyAsync();
        if (!companyResult.Success)
            return Unauthorized("Company profile not found.");

        var application = await _jobApplicationService.GetCompanyJobApplicationDetailsAsync(applicationId, companyResult.Company!.Id);
        if (application == null)
        {
            return NotFound("Application or associated job post not found or unauthorized.");
        }

        var fileResult = GetFilePathAndName(application, fileType);
        if (!fileResult.Success)
            return BadRequest(fileResult.ErrorMessage);

        if (!System.IO.File.Exists(fileResult.FilePath))
        {
            return NotFound($"The requested {fileType} file was not found.");
        }

        var fileBytes = await System.IO.File.ReadAllBytesAsync(fileResult.FilePath);
        return File(fileBytes, "application/pdf", fileResult.FileName);
    }

    #region Private Helper Methods

    private async Task<(bool Success, TechBoard.Models.Domain.Company? Company, IActionResult? Result)> GetCurrentCompanyAsync()
    {
        var companyUser = await _userManager.GetUserAsync(User);
        if (companyUser is not TechBoard.Models.Domain.Company company)
        {
            TempData["ErrorMessage"] = "Could not identify your company profile.";
            var result = RedirectToAction("Index", "CompanyDashboard", new { controller = "CompanyDashboard" });
            return (false, null, result);
        }

        return (true, company, null);
    }

    private async Task<Models.Domain.JobPost?> ValidateJobPostAccessAsync(int jobPostId, string companyId)
    {
        var jobPost = await _jobPostService.GetJobPostByIdAsync(jobPostId);
        return jobPost?.CompanyId == companyId ? jobPost : null;
    }

    private static List<CompanyJobApplicationViewModel> MapToCompanyJobApplicationViewModels(
        IEnumerable<JobApplication> applications, Models.Domain.JobPost jobPost)
    {
        return applications.Select(a => MapToCompanyJobApplicationViewModel(a, jobPost.Title)).ToList();
    }

    private static CompanyJobApplicationViewModel MapToCompanyJobApplicationViewModel(JobApplication application, string? jobTitle = null)
    {
        return new CompanyJobApplicationViewModel
        {
            Id = application.Id,
            JobTitle = jobTitle ?? application.JobPost.Title,
            ApplicantName = $"{application.User.FirstName} {application.User.LastName}",
            ApplicantEmail = application.User.Email!,
            AppliedDate = application.AppliedDate,
            Status = application.Status.ToString(),
            CoverLetterFileName = application.CoverLetterFileName,
            CoverLetterFilePath = application.CoverLetterFilePath,
            ResumeFileName = application.ResumeFileName,
            ResumeFilePath = application.ResumeFilePath,
            ApplicantNotes = application.ApplicantNotes,
            CompanyNotes = application.CompanyNotes
        };
    }

    private static List<SelectListItem> CreateStatusSelectList(ApplicationStatus currentStatus)
    {
        return Enum.GetValues(typeof(ApplicationStatus))
                   .Cast<ApplicationStatus>()
                   .Select(s => new SelectListItem
                   {
                       Value = s.ToString(),
                       Text = s.ToString(),
                       Selected = s == currentStatus
                   }).ToList();
    }

    private void SetTempDataMessage(bool success, string message)
    {
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = message;
    }

    private static (bool Success, string FilePath, string FileName, string ErrorMessage) GetFilePathAndName(
        JobApplication application, string fileType)
    {
        return fileType.ToLowerInvariant() switch
        {
            "resume" => (true, application.ResumeFilePath!, application.ResumeFileName!, string.Empty),
            "coverletter" => (true, application.CoverLetterFilePath!, application.CoverLetterFileName!, string.Empty),
            _ => (false, string.Empty, string.Empty, "Invalid file type specified.")
        };
    }

    #endregion
}