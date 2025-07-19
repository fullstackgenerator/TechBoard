using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechBoard.Constants;
using TechBoard.Models.Domain;

namespace TechBoard.Controllers.User;

[Route("user/auth")]
public class UserAuthController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public UserAuthController(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    [Authorize(Roles = Roles.User)]
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}