using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechBoard.ViewModels.Company.Account;
using TechBoard.ViewModels.Company.Profile;

namespace TechBoard.Controllers.Company
{
    [Authorize]
    public class CompanyAccountController : Controller
    {
        private readonly SignInManager<Models.Domain.Company> _signInManager;
        private readonly UserManager<Models.Domain.Company> _userManager;

        public CompanyAccountController(
            SignInManager<Models.Domain.Company> signInManager,
            UserManager<Models.Domain.Company> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangeCompanyPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Login", "CompanyLogin");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.SignOutAsync();
                await _signInManager.SignInAsync(user, isPersistent: false);

                TempData["PasswordChangeSuccessMessage"] = "Password changed successfully.";
                return RedirectToAction("Dashboard");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            TempData["PasswordChangeErrorMessage"] = "Failed to change password.";
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Login", "CompanyLogin");
            }

            var viewModel = new EditCompanyProfileViewModel
            {
                Name = user.Name,
                Address = user.Address,
                PostalCode = user.PostalCode,
                City = user.City,
                Country = user.Country,
                Phone = user.Phone,
                Email = user.Email
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditCompanyProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Login", "CompanyLogin");
            }

            user.Name = model.Name;
            user.Address = model.Address;
            user.PostalCode = model.PostalCode;
            user.City = model.City;
            user.Country = model.Country;
            user.Phone = model.Phone;
            user.Email = model.Email;
            user.UserName = model.Email;

            var updateResult = await _userManager.UpdateAsync(user);

            if (updateResult.Succeeded)
            {
                TempData["ProfileEditSuccessMessage"] = "Your profile has been updated successfully.";
                return RedirectToAction("Dashboard");
            }

            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, $"Error updating profile: {error.Description}");
            }

            return View(model);
        }
    }
}
