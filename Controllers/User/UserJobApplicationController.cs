using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechBoard.Constants;
using TechBoard.Models.Domain;
using TechBoard.Services;
using TechBoard.ViewModels.JobApplication;
using TechBoard.ViewModels.User.Profile;

namespace TechBoard.Controllers.User
{
    [Authorize(Roles = Roles.User)]
    [Route("user/applications")]
    public class UserJobApplicationController : Controller
    {
        private readonly IJobApplicationService _jobApplicationService;
        private readonly IJobPostService _jobPostService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserJobApplicationController> _logger;

        public UserJobApplicationController(
            IJobApplicationService jobApplicationService,
            IJobPostService jobPostService,
            UserManager<ApplicationUser> userManager,
            ILogger<UserJobApplicationController> logger)
        {
            _jobApplicationService = jobApplicationService;
            _jobPostService = jobPostService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: user/applications/apply/{jobPostId}
        [HttpGet("apply/{jobPostId}")]
        public async Task<IActionResult> Apply(int jobPostId)
        {
            var jobPost = await _jobPostService.GetJobPostByIdAsync(jobPostId);
            if (jobPost == null)
            {
                TempData["ErrorMessage"] = "Job post not found.";
                return RedirectToAction("Index", "PublicJobPost");
            }
            
            var userId = _userManager.GetUserId(User);
            if (userId != null && await _jobApplicationService.HasUserAppliedAsync(userId, jobPostId)) 
            {
                TempData["InfoMessage"] = "You have already applied for this job.";
                return RedirectToAction("Details", "PublicJobPost", new { id = jobPostId });
            }

            ViewData["JobTitle"] = jobPost.Title;
            ViewData["JobPostId"] = jobPostId;
            return View(new SubmitJobApplicationViewModel());
        }

        // POST: user/applications/apply/{jobPostId}
        [HttpPost("apply/{jobPostId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(int jobPostId, SubmitJobApplicationViewModel model)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to apply for a job.";
                return RedirectToAction("Login", "UserAuth");
            }
            
            var jobPost = await _jobPostService.GetJobPostByIdAsync(jobPostId);
            if (jobPost == null)
            {
                TempData["ErrorMessage"] = "Job post not found.";
                return RedirectToAction("Index", "PublicJobPost");
            }

            ViewData["JobTitle"] = jobPost.Title;
            ViewData["JobPostId"] = jobPostId;

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ApplyForJob ViewModel validation failed for user {UserId} and job {JobPostId}", userId, jobPostId);
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
                return View(model);
            }
        }

        // GET: user/applications/allapplications
        [Route("AllApplications")]
        public async Task<IActionResult> AllApplications()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to view your applications.";
                return RedirectToAction("Login", "UserAuth");
            }

            var applications = await _jobApplicationService.GetApplicationsByUserIdAsync(userId);
            var model = applications.Select(a => new JobApplicationViewModel
            {
                Id = a.Id,
                JobTitle = a.JobPost.Title,
                CompanyName = a.JobPost.Company.Name,
                AppliedDate = a.AppliedDate,
                Status = a.Status.ToString()
            }).ToList();

            return View(model);
        }
        
// GET: user/applications/details/{id}
        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to view application details.";
                return RedirectToAction("Login", "UserAuth");
            }

            var application = await _jobApplicationService.GetApplicationDetailsAsync(id);

            if (application == null || application.UserId != userId)
            {
                TempData["ErrorMessage"] = "Application not found or you don't have permission to view it.";
                return NotFound();
            }

            var model = new JobApplicationViewModel
            {
                Id = application.Id,
                JobTitle = application.JobPost.Title,
                CompanyName = application.JobPost.Company.Name,
                CoverLetter = application.CoverLetter,
                CvFileName = application.CVFileName,
                ResumeFileName = application.ResumeFileName,
                AppliedDate = application.AppliedDate,
                Status = application.Status.ToString(),
                CompanyNotes = application.CompanyNotes,
                JobPostId = application.JobPost.Id
            };

            return View(model);
        }    }
}