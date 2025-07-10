using System.ComponentModel.DataAnnotations;

namespace TechBoard.ViewModels.JobApplication
{
    public class SubmitJobApplicationViewModel
    {
        [Required(ErrorMessage = "A cover letter is required.")]
        [StringLength(5000, MinimumLength = 50, ErrorMessage = "Cover letter must be between 50 and 5000 characters.")]
        [Display(Name = "Cover Letter")]
        public string CoverLetter { get; set; } = null!;

        [Required(ErrorMessage = "Please upload your resume in PDF format.")]
        [DataType(DataType.Upload)]
        [MaxFileSize(5 * 1024 * 1024)]
        [AllowedExtensions(new[] { ".pdf" })]
        [Display(Name = "Resume (PDF)")]
        public IFormFile ResumeFile { get; set; } = null!;
    }
    
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxFileSize;
        public MaxFileSizeAttribute(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                if (file.Length > _maxFileSize)
                {
                    return new ValidationResult(GetErrorMessage());
                }
            }
            return ValidationResult.Success;
        }

        private string GetErrorMessage()
        {
            return $"Maximum allowed file size is {_maxFileSize / (1024 * 1024)} MB.";
        }
    }
    
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;
        public AllowedExtensionsAttribute(string[] extensions)
        {
            _extensions = extensions;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName);
                if (!_extensions.Contains(extension.ToLower()))
                {
                    return new ValidationResult(GetErrorMessage());
                }
            }
            return ValidationResult.Success;
        }

        private string GetErrorMessage()
        {
            return $"This file extension is not allowed! Allowed extensions are: {string.Join(", ", _extensions)}";
        }
    }
}