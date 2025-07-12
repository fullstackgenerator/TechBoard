using System.ComponentModel.DataAnnotations;

namespace TechBoard.ViewModels.Company.JobApplication
{
    public class CompanyJobApplicationViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Job Title")]
        public string JobTitle { get; set; } = null!;
        [Display(Name = "Applicant Name")]
        public string ApplicantName { get; set; } = null!;
        [Display(Name = "Applicant Email")]
        public string ApplicantEmail { get; set; } = null!;
        [Display(Name = "Applied Date")]
        [DisplayFormat(DataFormatString = "{0:dd. MM. yyyy}", ApplyFormatInEditMode = true)]
        public DateTime AppliedDate { get; set; }
        public string Status { get; set; } = null!;
        
        [Display(Name = "Cover Letter")]
        public string? CoverLetterFileName { get; set; }
        [Display(Name = "Resume")]
        public string? ResumeFileName { get; set; }
        
        public string? CoverLetterFilePath { get; set; }
        public string? ResumeFilePath { get; set; }
        
        [Display(Name = "Applicant Notes")]
        public string? ApplicantNotes { get; set; }
        
        [Display(Name = "Company Notes")]
        public string? CompanyNotes { get; set; }
    }
}