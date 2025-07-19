using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechBoard.Constants;
using TechBoard.Models.Domain;

namespace TechBoard.Controllers.Company;

[Route("company/auth")]
public class CompanyAuthController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public CompanyAuthController(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    [Authorize(Roles = Roles.Company)]
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}