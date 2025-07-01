using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechBoard.Constants;
using TechBoard.Models.Domain;
// Note: Changed from Account to Profile for consistency, assuming ChangeCompanyPasswordViewModel
// is mostly for CompanyProfile operations, though it can live under Account.
using TechBoard.ViewModels.Company.Account; // Still needed for ChangeCompanyPasswordViewModel
using TechBoard.ViewModels.Company.Profile;

namespace TechBoard.Controllers.Company;

[Authorize(Roles = Roles.Company)]
[Route("company/profile")] // Base route for all actions in this controller
public class CompanyProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public CompanyProfileController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // Displays the company profile details at /company/profile
    [HttpGet("")] // This makes "company/profile" map to this action
    [HttpGet("details")] // Also respond to "company/profile/details"
    public async Task<IActionResult> Details()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is not Models.Domain.Company company)
        {
            // Log this scenario or redirect to a more general error/access denied page
            return NotFound(); // Or Forbid(); if authenticated but not authorized
        }

        var model = new CompanyProfileViewModel
        {
            Name = company.Name,
            Address = company.Address,
            City = company.City,
            Country = company.Country,
            PostalCode = company.PostalCode,
            Phone = company.Phone,
            IdNumber = company.IdNumber,
            Email = company.Email!,
            Website = company.Website,
            Description = company.Description,
            EmployeeCount = company.EmployeeCount
        };

        return View(model);
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
        
        if (company.Email != model.Email)
        {
             var setEmailResult = await _userManager.SetEmailAsync(company, model.Email);
             if (!setEmailResult.Succeeded)
             {
                 foreach (var error in setEmailResult.Errors)
                     ModelState.AddModelError(string.Empty, error.Description);
                 return View(model);
             }

             var setUserNameResult = await _userManager.SetUserNameAsync(company, model.Email);
             if (!setUserNameResult.Succeeded)
             {
                 foreach (var error in setUserNameResult.Errors)
                     ModelState.AddModelError(string.Empty, error.Description);
                 return View(model);
             }
        }


        var result = await _userManager.UpdateAsync(company);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }
        
        await _signInManager.RefreshSignInAsync(user);

        TempData["StatusMessage"] = "Profile updated successfully.";
        return RedirectToAction("Details");
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
        if (!ModelState.IsValid)
        {
            TempData["PasswordChangeErrorMessage"] = "Please correct the errors in the form."; 
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            TempData["PasswordChangeErrorMessage"] = "User not found. Please log in again.";
            return NotFound();
        }
        
        var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            TempData["PasswordChangeErrorMessage"] = "Failed to change password. Please check the errors below.";
            return View(model);
        }
        
        await _signInManager.RefreshSignInAsync(user); 
        TempData["PasswordChangeSuccessMessage"] = "Password changed successfully.";
        return RedirectToAction("ChangePassword");
    }
}