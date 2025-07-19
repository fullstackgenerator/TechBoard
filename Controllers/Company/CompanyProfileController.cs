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
    
    [HttpGet("")]
    [HttpGet("details")]
    public async Task<IActionResult> Details()
    {
        var company = await GetCurrentCompanyAsync();
        if (company == null)
        {
            return NotFound();
        }

        var model = MapToProfileViewModel(company);
        return View(model);
    }

    [HttpGet("edit")]
    public async Task<IActionResult> Edit()
    {
        var company = await GetCurrentCompanyAsync();
        if (company == null)
        {
            return NotFound();
        }

        var model = MapToEditViewModel(company);
        return View(model);
    }

    [HttpPost("edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditCompanyProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var company = await GetCurrentCompanyAsync();
        if (company == null)
        {
            return NotFound();
        }

        var updateResult = await UpdateCompanyProfileAsync(company, model);
        if (!updateResult.Success)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(model);
        }

        await _signInManager.RefreshSignInAsync(company);
        TempData["StatusMessage"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Details));
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
        
        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            TempData["PasswordChangeSuccessMessage"] = "Password changed successfully.";
            return RedirectToAction(nameof(ChangePassword));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        
        TempData["PasswordChangeErrorMessage"] = "Failed to change password. Please check the errors below.";
        return View(model);
    }

    #region Private Helper Methods

    private async Task<Models.Domain.Company?> GetCurrentCompanyAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return user as Models.Domain.Company;
    }

    private static CompanyProfileViewModel MapToProfileViewModel(Models.Domain.Company company)
    {
        return new CompanyProfileViewModel
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
    }

    private static EditCompanyProfileViewModel MapToEditViewModel(Models.Domain.Company company)
    {
        return new EditCompanyProfileViewModel
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
    }

    private async Task<UpdateResult> UpdateCompanyProfileAsync(Models.Domain.Company company, EditCompanyProfileViewModel model)
    {
        company.Name = model.Name;
        company.Address = model.Address;
        company.City = model.City;
        company.Country = model.Country;
        company.PostalCode = model.PostalCode;
        company.Phone = model.Phone;
        company.IdNumber = model.IdNumber;
        
        if (company.Email != model.Email)
        {
            var emailUpdateResult = await UpdateEmailAsync(company, model.Email);
            if (!emailUpdateResult.Success)
            {
                return emailUpdateResult;
            }
        }
        
        var result = await _userManager.UpdateAsync(company);
        
        return new UpdateResult 
        { 
            Success = result.Succeeded,
            Errors = result.Errors?.Select(e => e.Description).ToList() ?? new List<string>()
        };
    }

    private async Task<UpdateResult> UpdateEmailAsync(ApplicationUser user, string newEmail)
    {
        var setEmailResult = await _userManager.SetEmailAsync(user, newEmail);
        if (!setEmailResult.Succeeded)
        {
            return new UpdateResult 
            { 
                Success = false,
                Errors = setEmailResult.Errors.Select(e => e.Description).ToList()
            };
        }

        var setUserNameResult = await _userManager.SetUserNameAsync(user, newEmail);
        if (!setUserNameResult.Succeeded)
        {
            return new UpdateResult 
            { 
                Success = false,
                Errors = setUserNameResult.Errors.Select(e => e.Description).ToList()
            };
        }

        return new UpdateResult { Success = true };
    }

    #endregion

    #region Helper Classes

    private class UpdateResult
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    #endregion
}