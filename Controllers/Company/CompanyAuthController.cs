using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechBoard.Constants;
using TechBoard.Models.Domain;
using TechBoard.ViewModels.Company.Account;

namespace TechBoard.Controllers.Company;

[Route("company/auth")]
public class CompanyAuthController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public CompanyAuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet("register")]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost("register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(CompanyRegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var company = new Models.Domain.Company
        {
            UserName = model.Email,
            Email = model.Email,
            Name = model.Name,
            Address = model.Address,
            PostalCode = model.PostalCode,
            City = model.City,
            Country = model.Country,
            Phone = model.Phone,
            IdNumber = model.IdNumber,
            MembershipTierId = 1 // Default to basic tier
        };

        var result = await _userManager.CreateAsync(company, model.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(company, Roles.Company);
            await _signInManager.SignInAsync(company, isPersistent: false);
            return RedirectToAction("Index", "CompanyDashboard");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
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