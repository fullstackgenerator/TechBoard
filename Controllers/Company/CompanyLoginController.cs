using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechBoard.ViewModels.Company.Account;

namespace TechBoard.Controllers.Company;

public class CompanyLoginController : Controller
{
    private readonly SignInManager<Models.Domain.Company> _signInManager;
    private readonly ILogger<CompanyLoginController> _logger;

    public CompanyLoginController(
        SignInManager<Models.Domain.Company> signInManager,
        ILogger<CompanyLoginController> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return RedirectToAction("Index", "CompanyDashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(CompanyLoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} logged in successfully.", model.Email);
            return RedirectToAction("Dashboard", "CompanyAccount");
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User {Email} is locked out.", model.Email);
            ModelState.AddModelError(string.Empty, "Account locked out. Please try again later.");
        }
        else
        {
            _logger.LogWarning("Invalid login attempt for {Email}.", model.Email);
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
