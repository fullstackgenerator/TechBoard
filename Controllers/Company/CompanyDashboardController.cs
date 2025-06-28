using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TechBoard.Controllers.Company;

[Authorize]
public class CompanyDashboardController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}