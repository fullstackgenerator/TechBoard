using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechBoard.Constants;
using TechBoard.Models.Domain;
using TechBoard.ViewModels.Admin.Account;
using TechBoard.ViewModels.Admin.Profile;

namespace TechBoard.Controllers.Admin;

[Authorize(Roles = Roles.Admin)]
[Route("admin/profile")]
public class AdminProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AdminProfileController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }
    
    [HttpGet("details")]
    public async Task<IActionResult> Details()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("User not found.");
        }
        
        var model = new AdminProfileViewModel
        {
            FullName = user.Name,
            Email = user.Email!,
        };

        return View(model);
    }


    [HttpGet("edit")]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("User not found.");

        var model = new EditAdminProfileViewModel()
        {
            FullName = user.Name,
            Email = user.Email!
        };

        return View(model);
    }

    [HttpPost("edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditAdminProfileViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("User not found.");
        
        user.Name = model.FullName;
        
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
    public async Task<IActionResult> ChangePassword(ChangeAdminPasswordViewModel model)
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