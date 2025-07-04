using System.ComponentModel.DataAnnotations;

namespace TechBoard.ViewModels.JobPost;

public class EditJobPostViewModel
{
    public int Id { get; set; }

    [Display(Name = "Job Title")]
    public string Title { get; set; } = null!;

    [Display(Name = "Description")]
    public string Description { get; set; } = null!;

    [Display(Name = "Location")]
    public string Location { get; set; } = null!;

    [Display(Name = "Posted On")]
    [DisplayFormat(DataFormatString = "{0:dd.mm yyyy}")]
    public DateTime PostedDate { get; set; }
}