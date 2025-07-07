using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechBoard.Constants;

namespace TechBoard.Controllers.User;

[Authorize(Roles = Roles.User)]
[Route("user/dashboard")]
public class UserDashboardController : Controller
{

    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }
}