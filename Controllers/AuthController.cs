using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechBoard.Models.Domain;
using TechBoard.Constants;
using TechBoard.ViewModels;
using TechBoard.Services;

namespace TechBoard.Controllers;

[Route("[controller]")]
public class AuthController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<AuthController> logger, IEmailService emailService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
        _emailService = emailService;
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
            result = await _userManager.CreateAsync(company, model.Company.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(company, Roles.Company);
                await _signInManager.SignOutAsync();

                TempData["SuccessMessage"] = "Registration successful! Please log in.";
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
            result = await _userManager.CreateAsync(user, model.User.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, Roles.User);
                
                await _signInManager.SignOutAsync();

                TempData["SuccessMessage"] = "Registration successful! Please log in.";
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
        
        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }
        
        if (user.IsBlocked)
        {
            _logger.LogWarning($"Blocked user '{user.Email}' attempted to log in.");
            ModelState.AddModelError(string.Empty, "Your account has been blocked. Please contact support for more information.");
            await _signInManager.SignOutAsync();
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
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

        else
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }
    }

    // POST /Auth/Logout
    [HttpPost("Logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        TempData.Clear(); 
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
     [HttpGet("ForgotPassword")]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost("ForgotPassword")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action(nameof(ResetPassword), "Auth", 
                new { email = model.Email, code }, 
                protocol: HttpContext.Request.Scheme);

            try
            {
                if (callbackUrl != null)
                {
                    await _emailService.SendPasswordResetEmailAsync(model.Email, callbackUrl);
                    _logger.LogInformation($"Password reset email sent to {model.Email}");
                    _logger.LogInformation($"Reset URL: {callbackUrl}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email");
            }

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        return View(model);
    }

    [HttpGet("ForgotPasswordConfirmation")]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [HttpGet("ResetPassword")]
    public IActionResult ResetPassword(string? code = null, string? email = null)
    {
        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(email))
        {
            return BadRequest("A code and email must be supplied for password reset.");
        }

        var model = new ResetPasswordViewModel
        {
            Code = code,
            Email = email
        };

        return View(model);
    }

    [HttpPost("ResetPassword")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
        if (result.Succeeded)
        {
            _logger.LogInformation($"Password successfully reset for user {model.Email}");
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpGet("ResetPasswordConfirmation")]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }
}