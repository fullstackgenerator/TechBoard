using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechBoard.Constants;
using TechBoard.Models.Domain;
using TechBoard.Services;

namespace TechBoard.Controllers.Company;

[Authorize(Roles = Roles.Company)]
[Route("company/analytics")]
public class CompanyAnalyticsController : Controller
{
    private readonly ILogger<CompanyAnalyticsController> _logger;
    private readonly ICompanyAnalyticsService _companyAnalyticsService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMembershipService _membershipService;

    public CompanyAnalyticsController(
        ILogger<CompanyAnalyticsController> logger,
        ICompanyAnalyticsService companyAnalyticsService,
        UserManager<ApplicationUser> userManager,
        IMembershipService membershipService)
    {
        _logger = logger;
        _companyAnalyticsService = companyAnalyticsService;
        _userManager = userManager;
        _membershipService = membershipService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var companyUser = await _userManager.GetUserAsync(User);
        if (companyUser is not Models.Domain.Company company)
        {
            _logger.LogWarning("Non-company user attempted to access analytics. User ID: {UserId}", _userManager.GetUserId(User));
            TempData["ErrorMessage"] = "Could not identify your company profile.";
            return RedirectToAction("Index", "CompanyDashboard");
        }
        
        var currentTier = await _membershipService.GetMembershipTierByIdAsync(company.MembershipTierId);
        if (!(currentTier?.CanAccessAnalytics ?? false))
        {
            TempData["ErrorMessage"] = "Your current membership tier does not include analytics. Please upgrade to access this feature.";
            return RedirectToAction("Details", "CompanyMembership");
        }

        var viewModel = await _companyAnalyticsService.GetCompanyAnalyticsAsync(company.Id);
        return View(viewModel);
    }
}