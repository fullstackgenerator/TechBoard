using System.ComponentModel.DataAnnotations;

namespace TechBoard.Models.Domain;

public enum WorkType
{
    [Display(Name = "Full-Time")]
    FullTime = 1,
    [Display(Name = "Part-Time")]
    PartTime = 2,
    [Display(Name = "Contract")]
    Contract = 3,
    [Display(Name = "Freelance")]
    Freelance = 4,
    [Display(Name = "Internship")]
    Internship = 5
}