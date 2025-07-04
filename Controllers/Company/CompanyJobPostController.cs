using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechBoard.Constants;
using TechBoard.Data;
using TechBoard.Services;
using TechBoard.Models;
using TechBoard.Models.Domain;

namespace TechBoard.Controllers.Company;

[Authorize(Roles = Roles.Company)]
public class CompanyJobPostController : Controller
{
    private readonly ILogger<CompanyJobPostController>? _logger;
    private readonly IJobPostService _jobPostService;
    private readonly ApplicationDbContext _dbContext;


    public CompanyJobPostController(IJobPostService jobPostService, ApplicationDbContext dbContext)
    {
        _jobPostService = jobPostService;
        _dbContext = dbContext;
        _logger = _logger;
    }

    [HttpGet("")]
    [Route("company/job-post")]
    public IActionResult JobPost()
    {
        return View("JobPost");
    }

    [HttpGet]
    [Route("company/new-post")]
    public IActionResult NewPost()
    {
        return View("NewPost");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind(
            "Title,Description,Location,PostedDate")]
        JobPost jobPost)
    {
        if (!ModelState.IsValid)
        {
 
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                return View("JobPost");
            }
        }

        _dbContext.Add(jobPost);
        await _dbContext.SaveChangesAsync();
        return RedirectToAction("JobPost");
    }
}