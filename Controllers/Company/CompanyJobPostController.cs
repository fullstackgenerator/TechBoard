using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechBoard.Constants;
using TechBoard.Data;
using TechBoard.Services;

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
        return View("JobPost", "JobPostViewModel");
    }
    
    [HttpGet]
    [Route("company/new-post")]
    public IActionResult NewPost()
    {
        return View("NewPost", "CreateJobPostViewModel");
    }
}
