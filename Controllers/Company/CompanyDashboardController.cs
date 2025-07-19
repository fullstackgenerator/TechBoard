using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechBoard.Constants;
using TechBoard.Models.Domain;

namespace TechBoard.Controllers.Company;

[Authorize(Roles = Roles.Company)]
[Route("company/dashboard")]
public class CompanyDashboardController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public CompanyDashboardController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("")]
    [HttpGet("index")]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var company = user as Models.Domain.Company;
        
        if (company == null)
        {
            return NotFound();
        }
        
        ViewBag.CompanyName = company.Name;
        return View();
    }
}