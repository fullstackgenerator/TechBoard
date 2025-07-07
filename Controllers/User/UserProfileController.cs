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
        var privateUser = await _userManager.GetUserAsync(User);
        if (privateUser is not Models.Domain.User user)
        {
            return NotFound();
        }

        var model = new UserProfileViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Address = user.Address,
            City = user.City,
            Country = user.Country,
            PostalCode = user.PostalCode,
            Phone = user.Phone,
            Email = user.Email!,
        };

        return View(model);
    }


    [HttpGet("edit")]
    public async Task<IActionResult> Edit()
    {
        var privateUser = await _userManager.GetUserAsync(User);
        if (privateUser is not Models.Domain.User user)
            return NotFound();

        var model = new EditUserProfileViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Address = user.Address,
            City = user.City,
            Country = user.Country,
            PostalCode = user.PostalCode,
            Phone = user.Phone,
            Email = user.Email!
        };

        return View(model);
    }

    [HttpPost("edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserProfileViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var privateUser = await _userManager.GetUserAsync(User);
        if (privateUser is not Models.Domain.User user)
            return NotFound();

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Address = model.Address;
        user.City = model.City;
        user.Country = model.Country;
        user.PostalCode = model.PostalCode;
        user.Phone = model.Phone;

        if (user.Email != model.Email)
        {
            var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
            if (!setEmailResult.Succeeded)
            {
                foreach (var error in setEmailResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            var setUserNameResult = await _userManager.SetUserNameAsync(user, model.Email);
            if (!setUserNameResult.Succeeded)
            {
                foreach (var error in setUserNameResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }
        }

        var result = await _userManager.UpdateAsync(user);

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