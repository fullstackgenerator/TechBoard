using Microsoft.AspNetCore.Mvc;

namespace TechBoard.Controllers.Company;

[Route("company/analytics")]
public class CompanyAnalyticsController : Controller
{
    private readonly ILogger<CompanyAnalyticsController> _logger;

    public CompanyAnalyticsController(ILogger<CompanyAnalyticsController> logger)
    {
        _logger = logger;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }
}