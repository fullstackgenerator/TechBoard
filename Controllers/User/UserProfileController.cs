using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechBoard.Constants;
using TechBoard.Models.Domain;
using TechBoard.ViewModels.User.Account;
using TechBoard.ViewModels.User.Profile;

namespace TechBoard.Controllers.User;

[Authorize(Roles = Roles.User)]
[Route("user/profile")]
public class UserProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public UserProfileController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet("")]
    [HttpGet("details")]
    public async Task<IActionResult> Details()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            return NotFound();
        }

        var model = MapToProfileViewModel(user);
        return View(model);
    }

    [HttpGet("edit")]
    public async Task<IActionResult> Edit()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            return NotFound();
        }

        var model = MapToEditViewModel(user);
        return View(model);
    }

    [HttpPost("edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            return NotFound();
        }

        var updateResult = await UpdateUserProfileAsync(user, model);
        if (!updateResult.Success)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(model);
        }

        await _signInManager.RefreshSignInAsync(user);
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
    public async Task<IActionResult> ChangePassword(ChangeUserPasswordViewModel model)
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

    private async Task<Models.Domain.User?> GetCurrentUserAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return user as Models.Domain.User;
    }

    private static UserProfileViewModel MapToProfileViewModel(Models.Domain.User user)
    {
        return new UserProfileViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Address = user.Address,
            City = user.City,
            Country = user.Country,
            PostalCode = user.PostalCode,
            Phone = user.Phone,
            Email = user.Email!,
            GitHubProfile = user.GitHubProfile,
            LinkedInProfile = user.LinkedInProfile,
            Website = user.Website
        };
    }

    private static EditUserProfileViewModel MapToEditViewModel(Models.Domain.User user)
    {
        return new EditUserProfileViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Address = user.Address,
            City = user.City,
            Country = user.Country,
            PostalCode = user.PostalCode,
            Phone = user.Phone,
            Email = user.Email!,
            GitHubProfile = user.GitHubProfile,
            LinkedInProfile = user.LinkedInProfile,
            Website = user.Website
        };
    }

    private async Task<UpdateResult> UpdateUserProfileAsync(Models.Domain.User user, EditUserProfileViewModel model)
    {
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Address = model.Address;
        user.City = model.City;
        user.Country = model.Country;
        user.PostalCode = model.PostalCode;
        user.Phone = model.Phone;
        user.GitHubProfile = model.GitHubProfile;
        user.LinkedInProfile = model.LinkedInProfile;
        user.Website = model.Website;
        
        if (user.Email != model.Email)
        {
            var emailUpdateResult = await UpdateEmailAsync(user, model.Email);
            if (!emailUpdateResult.Success)
            {
                return emailUpdateResult;
            }
        }
        
        var result = await _userManager.UpdateAsync(user);
        
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