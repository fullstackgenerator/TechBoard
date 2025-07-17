using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechBoard.Constants;

namespace TechBoard.Controllers.Admin;

[Authorize(Roles = Roles.Admin)]
[Route("admin/dashboard")]
public class AdminDashboardController : Controller
{

    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }
}