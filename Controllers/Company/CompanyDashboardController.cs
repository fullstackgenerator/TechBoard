using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TechBoard.Controllers.Company;

[Authorize]
public class CompanyDashboardController : Controller
{
    [HttpGet("Dashboard", Name = "CompanyAuth")]
    public IActionResult Index()
    {
        return View("Dashboard", "CompanyAuth");
    }
}