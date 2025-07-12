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
            string? coverLetterPdfFileName;
            string? coverLetterPdfFilePath = null;

            try
            {
                string resumesFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "resumes");
                if (!Directory.Exists(resumesFolder))
                {
                    Directory.CreateDirectory(resumesFolder);
                }
                
                string coverLettersFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "coverletters");
                if (!Directory.Exists(coverLettersFolder))
                {
                    Directory.CreateDirectory(coverLettersFolder);
                }
                
                resumeFileName = Guid.NewGuid() + "_" + model.ResumeFile.FileName;
                resumeFilePath = Path.Combine(resumesFolder, resumeFileName);

                using (var fileStream = new FileStream(resumeFilePath, FileMode.Create))
                {
                    await model.ResumeFile.CopyToAsync(fileStream);
                }
                
                coverLetterPdfFileName = Guid.NewGuid() + "_" + model.CoverLetter.FileName;
                coverLetterPdfFilePath = Path.Combine(coverLettersFolder, coverLetterPdfFileName);

                using (var fileStream = new FileStream(coverLetterPdfFilePath, FileMode.Create))
                {
                    await model.CoverLetter.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading application files for user {UserId} and job {JobPostId}", userId, jobPostId);
                
                if (!string.IsNullOrEmpty(resumeFilePath) && File.Exists(resumeFilePath))
                {
                    File.Delete(resumeFilePath);
                }
                if (!string.IsNullOrEmpty(coverLetterPdfFilePath) && File.Exists(coverLetterPdfFilePath))
                {
                    File.Delete(coverLetterPdfFilePath);
                }

                return (false, "An error occurred while uploading your documents. Please try again.");
            }

            var jobApplication = new JobApplication
            {
                UserId = userId,
                JobPostId = jobPostId,
                ApplicantNotes = model.Notes,
                CoverLetterFileName = coverLetterPdfFileName,
                CoverLetterFilePath = coverLetterPdfFilePath,
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
                if (!string.IsNullOrEmpty(coverLetterPdfFilePath) && File.Exists(coverLetterPdfFilePath))
                {
                    File.Delete(coverLetterPdfFilePath);
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
            // user's view of their own application
            return await _jobApplicationRepository.GetByIdAsync(applicationId);
        }
        
        public async Task<JobApplication?> GetCompanyJobApplicationDetailsAsync(int applicationId, string companyId)
        {
            var application = await _jobApplicationRepository.GetByIdAsync(applicationId);
            if (application == null)
            {
                return null;
            }

            // ensure application belongs to a job post owned by this company
            var jobPost = await _jobPostRepository.GetByIdAsync(application.JobPostId);
            if (jobPost == null || jobPost.CompanyId != companyId)
            {
                return null;
            }

            return application;
        }

        public async Task<IEnumerable<JobApplication>> GetApplicationsByCompanyJobPostAsync(string companyId, int jobPostId)
        {

            var jobPost = await _jobPostRepository.GetByIdAsync(jobPostId);
            if (jobPost == null || jobPost.CompanyId != companyId)
            {
                _logger.LogWarning("Company {CompanyId} attempted to access applications for non-existent or unauthorized job post {JobPostId}", companyId, jobPostId);
                return new List<JobApplication>();
            }
            
            return await _jobApplicationRepository.GetByJobPostIdAsync(jobPostId);
        }

        public async Task<bool> HasUserAppliedAsync(string userId, int jobPostId)
        {
            return await _jobApplicationRepository.HasUserAppliedAsync(userId, jobPostId);
        }

        public async Task<(bool Success, string Message)> UpdateJobApplicationStatusAsync(int applicationId, string companyId, ApplicationStatus newStatus)
        {
            var application = await GetCompanyJobApplicationDetailsAsync(applicationId, companyId);
            if (application == null)
            {
                return (false, "Application not found or you do not have permission to update it.");
            }

            application.Status = newStatus;
            application.Updated = DateTime.UtcNow;
            try
            {
                await _jobApplicationRepository.UpdateAsync(application);
                return (true, "Application status updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating application status for application {ApplicationId} by company {CompanyId}", applicationId, companyId);
                return (false, "An error occurred while updating the application status.");
            }
        }

        public async Task<(bool Success, string Message)> UpdateCompanyNotesAsync(int applicationId, string companyId, string notes)
        {
            var application = await GetCompanyJobApplicationDetailsAsync(applicationId, companyId);
            if (application == null)
            {
                return (false, "Application not found or you do not have permission to update notes.");
            }

            application.CompanyNotes = notes;
            application.Updated = DateTime.UtcNow;
            try
            {
                await _jobApplicationRepository.UpdateAsync(application);
                return (true, "Company notes updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating company notes for application {ApplicationId} by company {CompanyId}", applicationId, companyId);
                return (false, "An error occurred while updating company notes.");
            }
        }
    }
}