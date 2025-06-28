using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechBoard.Constants;
using TechBoard.Models.Domain;
using TechBoard.ViewModels.Company.Account;
using TechBoard.ViewModels.Company.Profile;

namespace TechBoard.Controllers.Company;

[Authorize(Roles = Roles.Company)]
[Route("company/profile")]
public class CompanyProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public CompanyProfileController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet("edit")]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is not Models.Domain.Company company) 
            return NotFound();

        var model = new EditCompanyProfileViewModel
        {
            Name = company.Name,
            Address = company.Address,
            City = company.City,
            Country = company.Country,
            PostalCode = company.PostalCode,
            Phone = company.Phone,
            IdNumber = company.IdNumber,
            Email = company.Email!
        };

        return View(model);
    }

    [HttpPost("edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditCompanyProfileViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user is not Models.Domain.Company company) 
            return NotFound();

        company.Name = model.Name;
        company.Address = model.Address;
        company.City = model.City;
        company.Country = model.Country;
        company.PostalCode = model.PostalCode;
        company.Phone = model.Phone;
        company.IdNumber = model.IdNumber;

        var result = await _userManager.UpdateAsync(company);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        TempData["StatusMessage"] = "Profile updated successfully.";
        return RedirectToAction("Edit");
    }

    [HttpGet("change-password")]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost("change-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangeCompanyPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["StatusMessage"] = "Password changed successfully.";
        return RedirectToAction("ChangePassword");
    }
}