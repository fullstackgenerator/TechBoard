using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechBoard.Constants;
using TechBoard.Data;
using TechBoard.Models.Domain;
using TechBoard.ViewModels.Admin.Dashboard;

namespace TechBoard.Controllers.Admin;

[Authorize(Roles = Roles.Admin)]
[Route("admin/dashboard")]
public class AdminDashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminDashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var model = new AdminDashboardViewModel();

        // get overview statistics
        model.TotalUsers = await _userManager.Users.CountAsync();
        model.TotalCompanies = await _context.Companies.CountAsync();
        model.TotalJobPosts = await _context.JobPosts.CountAsync();
        model.TotalJobApplications = await _context.JobApplications.CountAsync();
        
        // Get ONLY regular users (not companies) for the Users list
        var regularUsers = await _userManager.Users
            .OfType<Models.Domain.User>()
            .Include(u => u.JobApplications)
            .Select(u => new UserManagementViewModel
            {
                Id = u.Id,
                Email = u.Email!,
                FullName = u.Name,
                IsCompany = false,
                IsBlocked = u.IsBlocked,
                CreatedDate = u.Created,
                JobApplicationsCount = u.JobApplications.Count
            })
            .OrderBy(u => u.CreatedDate)
            .ToListAsync();

        model.Users = regularUsers;

        // get companies for management (separate list)
        var companies = await _context.Companies
            .Include(c => c.MembershipTier)
            .Include(c => c.JobPosts)
                .ThenInclude(jp => jp.JobApplications)
            .ToListAsync();

        model.Companies = companies.Select(c => new CompanyManagementViewModel
        {
            Id = c.Id,
            CompanyName = c.Name,
            Email = c.Email!,
            IsBlocked = c.IsBlocked,
            TotalJobPosts = c.JobPosts.Count,
            ActiveJobPosts = c.JobPosts.Count(jp => jp.IsActive),
            TotalApplicationsReceived = c.JobPosts.Sum(jp => jp.JobApplications.Count),
            MembershipTier = c.MembershipTier.Name,
            CreatedDate = c.Created
        }).ToList();

        // get job posts for management
        var jobPosts = await _context.JobPosts
            .Include(jp => jp.Company)
            .Include(jp => jp.JobApplications)
            .ToListAsync();

        model.JobPosts = jobPosts.Select(jp => new JobPostManagementViewModel
        {
            Id = jp.Id,
            Title = jp.Title,
            CompanyName = jp.Company.Name,
            CompanyId = jp.CompanyId,
            PostedDate = jp.PostedDate,
            IsActive = jp.IsActive,
            ViewCount = jp.ViewCount,
            ApplicationsCount = jp.JobApplications.Count,
            IsFeatured = jp.IsFeatured
        }).ToList();

        return View(model);
    }

    // toggle user/company block status
    [HttpPost("toggle-block/{id}")]
    public async Task<IActionResult> ToggleBlock(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("Index");
        }

        user.IsBlocked = !user.IsBlocked;
        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = $"User '{user.Email}' block status toggled to {(!user.IsBlocked ? "unblocked" : "blocked")}.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to toggle block status.";
            foreach (var error in result.Errors)
            {
                TempData["ErrorMessage"] += $" {error.Description}";
            }
        }
        return RedirectToAction("Index");
    }
}