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
        var companyUser = await _userManager.GetUserAsync(User);
        if (companyUser is not TechBoard.Models.Domain.Company company)
        {
            TempData["ErrorMessage"] = "Could not identify your company profile.";
            return RedirectToAction("Index", "CompanyDashboard", new { controller = "CompanyDashboard" });
        }

        var jobPost = await _jobPostService.GetJobPostByIdAsync(jobPostId);
        if (jobPost == null || jobPost.CompanyId != company.Id)
        {
            TempData["ErrorMessage"] = "Job post not found or you do not have permission to view its applications.";
            return NotFound();
        }

        var applications = await _jobApplicationService.GetApplicationsByCompanyJobPostAsync(company.Id, jobPostId);

        var model = applications.Select(a => new CompanyJobApplicationViewModel
        {
            Id = a.Id,
            JobTitle = jobPost.Title,
            ApplicantName = $"{(a.User).FirstName} {(a.User).LastName}",
            ApplicantEmail = a.User.Email!,
            AppliedDate = a.AppliedDate,
            Status = a.Status.ToString(),
            CoverLetterFileName = a.CoverLetterFileName,
            CoverLetterFilePath = a.CoverLetterFilePath,
            ResumeFileName = a.ResumeFileName,
            ResumeFilePath = a.ResumeFilePath,
            ApplicantNotes = a.ApplicantNotes,
            CompanyNotes = a.CompanyNotes
        }).ToList();

        ViewData["JobPostTitle"] = jobPost.Title;
        ViewData["JobPostId"] = jobPostId;

        return View(model);
    }

    // GET: /company/jobposts/applications/{applicationId}/details
    [HttpGet("applications/{applicationId}/details")]
    public async Task<IActionResult> ApplicationDetails(int applicationId)
    {
        var companyUser = await _userManager.GetUserAsync(User);
        if (companyUser is not TechBoard.Models.Domain.Company company)
        {
            TempData["ErrorMessage"] = "Could not identify your company profile.";
            return RedirectToAction("Index", "CompanyDashboard", new { controller = "CompanyDashboard" });
        }

        var application = await _jobApplicationService.GetCompanyJobApplicationDetailsAsync(applicationId, company.Id);

        if (application == null)
        {
            TempData["ErrorMessage"] = "Application not found or you do not have permission to view it.";
            return NotFound();
        }
        
        var model = new CompanyJobApplicationViewModel
        {
            Id = application.Id,
            JobTitle = application.JobPost.Title,
            ApplicantName = $"{(application.User).FirstName} {application.User.LastName}",
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
        
        ViewBag.StatusList = Enum.GetValues(typeof(ApplicationStatus))
                                 .Cast<ApplicationStatus>()
                                 .Select(s => new SelectListItem
                                 {
                                     Value = s.ToString(),
                                     Text = s.ToString(),
                                     Selected = s == application.Status
                                 }).ToList();

        ViewData["JobPostId"] = application.JobPostId;

        return View(model);
    }

    // POST: /company/jobposts/applications/{applicationId}/update-status
    [HttpPost("applications/{applicationId}/update-status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateApplicationStatus(int applicationId, [FromForm] ApplicationStatus newStatus)
    {
        var companyUser = await _userManager.GetUserAsync(User);
        if (companyUser is not TechBoard.Models.Domain.Company company)
        {
            TempData["ErrorMessage"] = "Could not identify your company profile.";
            return RedirectToAction("Index", "CompanyDashboard", new { controller = "CompanyDashboard" });
        }

        var (success, message) = await _jobApplicationService.UpdateJobApplicationStatusAsync(applicationId, company.Id, newStatus);

        if (success)
        {
            TempData["SuccessMessage"] = message;
        }
        else
        {
            TempData["ErrorMessage"] = message;
        }

        return RedirectToAction("ApplicationDetails", new { applicationId });
    }
    
    // POST: /company/jobposts/applications/{applicationId}/update-notes
    [HttpPost("applications/{applicationId}/update-notes")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCompanyNotes(int applicationId, [FromForm] string companyNotes)
    {
        var companyUser = await _userManager.GetUserAsync(User);
        if (companyUser is not TechBoard.Models.Domain.Company company)
        {
            TempData["ErrorMessage"] = "Could not identify your company profile.";
            return RedirectToAction("Index", "CompanyDashboard", new { controller = "CompanyDashboard" });
        }

        var (success, message) = await _jobApplicationService.UpdateCompanyNotesAsync(applicationId, company.Id, companyNotes);

        if (success)
        {
            TempData["SuccessMessage"] = message;
        }
        else
        {
            TempData["ErrorMessage"] = message;
        }

        return RedirectToAction("ApplicationDetails", new { applicationId });
    }
    
    // GET: /company/jobposts/applications/download/{applicationId}/{fileType}
    [HttpGet("applications/download/{applicationId}/{fileType}")]
    public async Task<IActionResult> DownloadApplicationFile(int applicationId, string fileType)
    {
        var companyUser = await _userManager.GetUserAsync(User);
        if (companyUser is not TechBoard.Models.Domain.Company company)
        {
            return Unauthorized("Company profile not found.");
        }

        var application = await _jobApplicationService.GetCompanyJobApplicationDetailsAsync(applicationId, company.Id);

        if (application == null)
        {
            return NotFound("Application or associated job post not found or unauthorized.");
        }

        string? filePath;
        string? fileName;
        string contentType = "application/pdf";

        if (fileType.Equals("resume", StringComparison.OrdinalIgnoreCase))
        {
            filePath = application.ResumeFilePath;
            fileName = application.ResumeFileName;
        }
        else if (fileType.Equals("coverletter", StringComparison.OrdinalIgnoreCase))
        {
            filePath = application.CoverLetterFilePath;
            fileName = application.CoverLetterFileName;
        }
        else
        {
            return BadRequest("Invalid file type specified.");
        }

        if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
        {
            return NotFound($"The requested {fileType} file was not found.");
        }

        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        return File(fileBytes, contentType, fileName);
    }
}