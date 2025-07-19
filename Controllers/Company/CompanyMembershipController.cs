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
        var companyResult = await GetCurrentCompanyAsync();
        if (!companyResult.Success)
            return companyResult.Result!;

        var currentTier = await _membershipService.GetMembershipTierByIdAsync(companyResult.Company!.MembershipTierId);
        if (currentTier == null)
        {
            _logger.LogError("Company {CompanyId} assigned to non-existent membership tier ID: {TierId}", 
                companyResult.Company.Id, companyResult.Company.MembershipTierId);
            TempData["ErrorMessage"] = "Your current membership tier could not be found. Please contact support.";
            return RedirectToCompanyDashboard();
        }

        var availableTiers = await _membershipService.GetAllMembershipTiersAsync();
        var model = CreateMembershipViewModel(currentTier, availableTiers);

        return View(model);
    }

    // POST: /company/membership/upgrade/{tierId}
    [HttpPost("upgrade/{tierId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpgradeMembership(int tierId)
    {
        var companyResult = await GetCurrentCompanyAsync();
        if (!companyResult.Success)
            return companyResult.Result!;

        var (success, message) = await _membershipService.UpdateCompanyMembershipAsync(companyResult.Company!.Id, tierId);

        LogMembershipUpgradeAttempt(companyResult.Company.Id, tierId, success, message);
        SetTempDataMessage(success, message);

        return RedirectToAction("Details");
    }

    #region Private Helper Methods

    private async Task<(bool Success, TechBoard.Models.Domain.Company? Company, IActionResult? Result)> GetCurrentCompanyAsync()
    {
        var companyUser = await _userManager.GetUserAsync(User);
        if (companyUser is not TechBoard.Models.Domain.Company company)
        {
            _logger.LogWarning("Non-company user attempted to access membership details. User ID: {UserId}", 
                _userManager.GetUserId(User));
            TempData["ErrorMessage"] = "Could not identify your company profile.";
            return (false, null, RedirectToCompanyDashboard());
        }

        return (true, company, null);
    }

    private IActionResult RedirectToCompanyDashboard()
    {
        return RedirectToAction("Index", "CompanyDashboard", new { controller = "CompanyDashboard" });
    }

    private static CompanyMembershipViewModel CreateMembershipViewModel(
        MembershipTier currentTier, 
        IEnumerable<MembershipTier> availableTiers)
    {
        return new CompanyMembershipViewModel
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
            CurrentTierId = currentTier.Id,
            AvailableTiers = MapToMembershipTierViewModels(availableTiers)
        };
    }

    private static List<MembershipTierViewModel> MapToMembershipTierViewModels(IEnumerable<MembershipTier> tiers)
    {
        return tiers.Select(MapToMembershipTierViewModel).ToList();
    }

    private static MembershipTierViewModel MapToMembershipTierViewModel(MembershipTier tier)
    {
        return new MembershipTierViewModel
        {
            Id = tier.Id,
            Name = tier.Name,
            Description = tier.Description,
            Price = tier.Price,
            MaxJobPosts = tier.MaxJobPosts,
            JobPostDurationDays = tier.JobPostDurationDays,
            CanPostFeatured = tier.CanPostFeatured,
            CanAccessAnalytics = tier.CanAccessAnalytics,
            CanContactCandidates = tier.CanContactCandidates,
            MaxApplicationsPerJob = tier.MaxApplicationsPerJob
        };
    }

    private void LogMembershipUpgradeAttempt(string companyId, int tierId, bool success, string message)
    {
        if (success)
        {
            _logger.LogInformation("Company {CompanyId} successfully upgraded membership to tier ID {TierId}", 
                companyId, tierId);
        }
        else
        {
            _logger.LogWarning("Company {CompanyId} failed to upgrade membership to tier ID {TierId}: {Message}", 
                companyId, tierId, message);
        }
    }

    private void SetTempDataMessage(bool success, string message)
    {
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = message;
    }

    #endregion
}