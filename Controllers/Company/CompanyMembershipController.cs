using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechBoard.Constants;
using TechBoard.Models.Domain;
using TechBoard.Services;
using TechBoard.ViewModels.Company.Membership;

namespace TechBoard.Controllers.Company;

[Authorize(Roles = Roles.Company)]
[Route("company/membership")]
public class CompanyMembershipController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMembershipService _membershipService;
    private readonly ILogger<CompanyMembershipController> _logger;

    public CompanyMembershipController(
        UserManager<ApplicationUser> userManager,
        IMembershipService membershipService,
        ILogger<CompanyMembershipController> logger)
    {
        _userManager = userManager;
        _membershipService = membershipService;
        _logger = logger;
    }

    // GET: /company/membership
    [HttpGet("")]
    [HttpGet("details")]
    public async Task<IActionResult> Details()
    {
        var companyUser = await _userManager.GetUserAsync(User);
        if (companyUser is not Models.Domain.Company company)
        {
            _logger.LogWarning("Non-company user attempted to access membership details. User ID: {UserId}", _userManager.GetUserId(User));
            TempData["ErrorMessage"] = "Could not identify your company profile.";
            return RedirectToAction("Index", "CompanyDashboard", new { controller = "CompanyDashboard" });
        }

        var currentTier = await _membershipService.GetMembershipTierByIdAsync(company.MembershipTierId);
        if (currentTier == null)
        {
            _logger.LogError("Company {CompanyId} assigned to non-existent membership tier ID: {TierId}", company.Id, company.MembershipTierId);
            TempData["ErrorMessage"] = "Your current membership tier could not be found. Please contact support.";
            return RedirectToAction("Index", "CompanyDashboard", new { controller = "CompanyDashboard" });
        }

        var availableTiers = await _membershipService.GetAllMembershipTiersAsync();

        var model = new CompanyMembershipViewModel
        {
            CurrentTierName = currentTier.Name,
            CurrentTierDescription = currentTier.Description,
            CurrentTierPrice = currentTier.Price,
            CurrentTierMaxJobPosts = currentTier.MaxJobPosts,
            CurrentTierJobPostDurationDays = currentTier.JobPostDurationDays,
            CurrentTierCanPostFeatured = currentTier.CanPostFeatured,
            CurrentTierCanAccessAnalytics = currentTier.CanAccessAnalytics,
            CurrentTierCanContactCandidates = currentTier.CanContactCandidates,
            CurrentTierMaxApplicationsPerJob = currentTier.MaxApplicationsPerJob,
            AvailableTiers = availableTiers.Select(t => new MembershipTierViewModel
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Price = t.Price,
                MaxJobPosts = t.MaxJobPosts,
                JobPostDurationDays = t.JobPostDurationDays,
                CanPostFeatured = t.CanPostFeatured,
                CanAccessAnalytics = t.CanAccessAnalytics,
                CanContactCandidates = t.CanContactCandidates,
                MaxApplicationsPerJob = t.MaxApplicationsPerJob
            }).ToList(),
            CurrentTierId = currentTier.Id
        };

        return View(model);
    }

    // POST: /company/membership/upgrade/{tierId}
    [HttpPost("upgrade/{tierId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpgradeMembership(int tierId)
    {
        var companyUser = await _userManager.GetUserAsync(User);
        if (companyUser is not Models.Domain.Company company)
        {
            TempData["ErrorMessage"] = "Could not identify your company profile.";
            return RedirectToAction("Index", "CompanyDashboard", new { controller = "CompanyDashboard" });
        }

        var (success, message) = await _membershipService.UpdateCompanyMembershipAsync(company.Id, tierId);

        if (success)
        {
            TempData["SuccessMessage"] = message;
            _logger.LogInformation("Company {CompanyId} successfully upgraded membership to tier ID {TierId}", company.Id, tierId);
        }
        else
        {
            TempData["ErrorMessage"] = message;
            _logger.LogWarning("Company {CompanyId} failed to upgrade membership to tier ID {TierId}: {Message}", company.Id, tierId, message);
        }

        return RedirectToAction("Details");
    }
}