using System.ComponentModel.DataAnnotations;

namespace TechBoard.Models.Domain;

public enum Category
{
    Frontend = 1,
    Backend = 2,
    FullStack = 3,
    Mobile = 4,
    DevOps = 5,
    [Display(Name = "Data Science")]
    DataScience = 6,
    [Display(Name = "Machine Learning")]
    MachineLearning = 7,
    [Display(Name = "Cyber security")]
    Cybersecurity = 8,
    [Display(Name = "Cloud Engineering")]
    CloudEngineering = 9,
    [Display(Name = "Game Development")]
    GameDevelopment = 10,
    [Display(Name = "Quality Assurance")]
    QualityAssurance = 11,
    [Display(Name = "Product Management")]
    ProductManagement = 12,
    [Display(Name = "UX/UI Design")]
    UxuiDesign = 13,
    [Display(Name = "System Administration")]
    SystemAdministration = 14,
    [Display(Name = "Database Administration")]
    DatabaseAdministration = 15
}