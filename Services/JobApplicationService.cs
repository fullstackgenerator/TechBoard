using TechBoard.Models.Domain;
using TechBoard.Repositories;
using TechBoard.ViewModels.JobApplication;
using Microsoft.AspNetCore.Identity;

namespace TechBoard.Services
{
    public class JobApplicationService : IJobApplicationService
    {
        private readonly IJobApplicationRepository _jobApplicationRepository;
        private readonly IJobPostRepository _jobPostRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<JobApplicationService> _logger;

        public JobApplicationService(
            IJobApplicationRepository jobApplicationRepository,
            IJobPostRepository jobPostRepository,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment,
            ILogger<JobApplicationService> logger)
        {
            _jobApplicationRepository = jobApplicationRepository;
            _jobPostRepository = jobPostRepository;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task<(bool Success, string Message)> ApplyForJobAsync(
            int jobPostId,
            string userId,
            SubmitJobApplicationViewModel model)
        {
            var applicationUser = await _userManager.FindByIdAsync(userId);
            if (applicationUser is not User)
            {
                _logger.LogWarning("Attempt to apply for job by non-user ID: {UserId}", userId);
                return (false, "Only registered users can apply for jobs.");
            }

            var jobPost = await _jobPostRepository.GetByIdAsync(jobPostId);
            if (jobPost == null)
            {
                _logger.LogWarning("Attempt to apply for non-existent job post ID: {JobPostId}", jobPostId);
                return (false, "Job post not found.");
            }

            var hasApplied = await _jobApplicationRepository.HasUserAppliedAsync(userId, jobPostId);
            if (hasApplied)
            {
                return (false, "You have already applied for this job.");
            }

            string? resumeFileName;
            string? resumeFilePath = null;
            string? cvFileName;
            string? cvFilePath = null;

            try
            {
                // Directory for Resumes
                string resumesFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "resumes");
                if (!Directory.Exists(resumesFolder))
                {
                    Directory.CreateDirectory(resumesFolder);
                }

                // Directory for CVs
                string cvsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "cvs");
                if (!Directory.Exists(cvsFolder))
                {
                    Directory.CreateDirectory(cvsFolder);
                }

                // Upload Resume File
                resumeFileName = Guid.NewGuid() + "_" + model.ResumeFile.FileName;
                resumeFilePath = Path.Combine(resumesFolder, resumeFileName);

                using (var fileStream = new FileStream(resumeFilePath, FileMode.Create))
                {
                    await model.ResumeFile.CopyToAsync(fileStream);
                }

                // Upload CV File
                cvFileName = Guid.NewGuid() + "_" + model.CVFile.FileName;
                cvFilePath = Path.Combine(cvsFolder, cvFileName);

                using (var fileStream = new FileStream(cvFilePath, FileMode.Create))
                {
                    await model.CVFile.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading application files for user {UserId} and job {JobPostId}", userId, jobPostId);
                
                if (!string.IsNullOrEmpty(resumeFilePath) && File.Exists(resumeFilePath))
                {
                    File.Delete(resumeFilePath);
                }
                if (!string.IsNullOrEmpty(cvFilePath) && File.Exists(cvFilePath))
                {
                    File.Delete(cvFilePath);
                }

                return (false, "An error occurred while uploading your documents. Please try again.");
            }

            var jobApplication = new JobApplication
            {
                UserId = userId,
                JobPostId = jobPostId,
                CoverLetter = model.CoverLetter,
                CVFileName = cvFileName,
                CVFilePath = cvFilePath,
                ResumeFileName = resumeFileName,
                ResumeFilePath = resumeFilePath,
                AppliedDate = DateTime.UtcNow,
                Status = ApplicationStatus.Pending
            };

            try
            {
                await _jobApplicationRepository.CreateAsync(jobApplication);
                return (true, "Your application has been submitted successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting job application for user {UserId} and job {JobPostId}", userId, jobPostId);
                if (!string.IsNullOrEmpty(resumeFilePath) && File.Exists(resumeFilePath))
                {
                    File.Delete(resumeFilePath);
                }
                if (!string.IsNullOrEmpty(cvFilePath) && File.Exists(cvFilePath))
                {
                    File.Delete(cvFilePath);
                }
                return (false, "An error occurred while submitting your application. Please try again.");
            }
        }

        public async Task<IEnumerable<JobApplication>> GetApplicationsByUserIdAsync(string userId)
        {
            return await _jobApplicationRepository.GetByUserIdAsync(userId);
        }

        public async Task<JobApplication?> GetApplicationDetailsAsync(int applicationId)
        {
            return await _jobApplicationRepository.GetByIdAsync(applicationId);
        }

        public async Task<bool> HasUserAppliedAsync(string userId, int jobPostId)
        {
            return await _jobApplicationRepository.HasUserAppliedAsync(userId, jobPostId);
        }
    }
}