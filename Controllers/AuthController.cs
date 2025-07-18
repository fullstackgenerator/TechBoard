using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechBoard.Models.Domain;
using TechBoard.Constants;
using TechBoard.ViewModels;

namespace TechBoard.Controllers;

[Route("[controller]")]
public class AuthController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    // GET /Auth/Login
    [HttpGet("Login")]
    public IActionResult Login(string? returnUrl = null)
    {

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // POST /Auth/Login
    [HttpPost("Login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                if (await _userManager.IsInRoleAsync(user, Roles.Admin))
                {
        
                    return RedirectToLocal(returnUrl, "AdminDashboard", "Index");
                }
                else if (await _userManager.IsInRoleAsync(user, Roles.Company))
                {
                    return RedirectToLocal(returnUrl, "CompanyDashboard", "Index");
                }
                else if (await _userManager.IsInRoleAsync(user, Roles.User))
                {
                    return RedirectToLocal(returnUrl, "UserDashboard", "Index");
                }
                else
                {

                    return RedirectToLocal(returnUrl, "Home", "Index");
                }
            }
        }
        else if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account locked out.");
            return View(model);
        }
        else if (result.IsNotAllowed)
        {
            ModelState.AddModelError(string.Empty, "Login not allowed. Your account might not be confirmed.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    // POST /Auth/Logout
    [HttpPost("Logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
    
    private IActionResult RedirectToLocal(string? returnUrl, string defaultController, string defaultAction)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            return RedirectToAction(defaultAction, defaultController);
        }
    }
}