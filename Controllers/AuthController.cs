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
    private readonly RoleManager<IdentityRole> _roleManager;

    public AuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet("Register")]
    public IActionResult Register()
    {
        return View(new RegistrationViewModel());
    }

    [HttpPost("Register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegistrationViewModel model)
    {
        if (model.IsCompanyRegistration)
        {
            foreach (var key in ModelState.Keys.ToList())
            {
                if (key.StartsWith("User."))
                {
                    ModelState.Remove(key);
                }
            }
            if (string.IsNullOrWhiteSpace(model.Company.Email))
            {
                ModelState.AddModelError("Company.Email", "The Email field is required.");
            }
            if (string.IsNullOrWhiteSpace(model.Company.Password))
            {
                ModelState.AddModelError("Company.Password", "The Password field is required.");
            }
            if (string.IsNullOrWhiteSpace(model.Company.ConfirmPassword))
            {
                ModelState.AddModelError("Company.ConfirmPassword", "The Confirm Password field is required.");
            }
        }
        else
        {
            foreach (var key in ModelState.Keys.ToList())
            {
                if (key.StartsWith("Company."))
                {
                    ModelState.Remove(key);
                }
            }
            if (string.IsNullOrWhiteSpace(model.User.Email))
            {
                ModelState.AddModelError("User.Email", "The Email field is required.");
            }
            if (string.IsNullOrWhiteSpace(model.User.Password))
            {
                ModelState.AddModelError("User.Password", "The Password field is required.");
            }
            if (string.IsNullOrWhiteSpace(model.User.ConfirmPassword))
            {
                ModelState.AddModelError("User.ConfirmPassword", "The Confirm Password field is required.");
            }
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        IdentityResult result;
        ApplicationUser newUser;

        if (model.IsCompanyRegistration)
        {
            var company = new Models.Domain.Company
            {
                UserName = model.Company.Email,
                Email = model.Company.Email,
                Name = model.Company.Name,
                Address = model.Company.Address,
                PostalCode = model.Company.PostalCode,
                City = model.Company.City,
                Country = model.Company.Country,
                Phone = model.Company.Phone,
                IdNumber = model.Company.IdNumber,
                MembershipTierId = 1
            };
            newUser = company;
            result = await _userManager.CreateAsync(company, model.Company.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(company, Roles.Company);
                await _signInManager.SignOutAsync();
                
                return RedirectToAction("Login", "Auth");
            }
        }
        else
        {
            var user = new Models.Domain.User()
            {
                UserName = model.User.Email,
                Email = model.User.Email,
                FirstName = model.User.FirstName,
                LastName = model.User.LastName,
                Address = model.User.Address,
                PostalCode = model.User.PostalCode,
                City = model.User.City,
                Country = model.User.Country,
                Phone = model.User.Phone,
                Name = $"{model.User.FirstName} {model.User.LastName}"
            };
            newUser = user;
            result = await _userManager.CreateAsync(user, model.User.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, Roles.User);
                await _signInManager.SignOutAsync();
                
                return RedirectToAction("Login", "Auth");
            }
        }
        
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        
        return View(model);
    }
    
    // GET /Auth/Login
    [HttpGet("Login")]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }
    
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
            if (defaultController == "CompanyDashboard")
                return Redirect("/company/dashboard");
            if (defaultController == "UserDashboard")
                return Redirect("/user/dashboard");
            
            return RedirectToAction(defaultAction, defaultController);
        }
    }
}