using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechBoard.Constants;

namespace TechBoard.Controllers.Company;

[Authorize(Roles = Roles.Company)]
[Route("company/dashboard")]
public class CompanyDashboardController : Controller
{

    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }
}