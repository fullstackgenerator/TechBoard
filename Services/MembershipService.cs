using Microsoft.EntityFrameworkCore;
using TechBoard.Data;
using TechBoard.Models.Domain;
using Microsoft.AspNetCore.Identity;

namespace TechBoard.Services
{
    public class MembershipService : IMembershipService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<MembershipService> _logger;


        public MembershipService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<MembershipService> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IEnumerable<MembershipTier>> GetAllMembershipTiersAsync()
        {

            return (await _context.MembershipTiers.ToListAsync()).OrderBy(t => t.Price);
            
        }

        public async Task<MembershipTier?> GetMembershipTierByIdAsync(int id)
        {
            return await _context.MembershipTiers.FindAsync(id);
        }

        public async Task<MembershipTier?> GetMembershipTierByNameAsync(string name)
        {
            return await _context.MembershipTiers.FirstOrDefaultAsync(t => t.Name == name);
        }


        public async Task<(bool Success, string Message)> UpdateCompanyMembershipAsync(string companyId, int newMembershipTierId)
        {
            var company = await _userManager.FindByIdAsync(companyId) as Company;
            if (company == null)
            {
                _logger.LogWarning("Attempt to update membership for non-existent company ID: {CompanyId}", companyId);
                return (false, "Company not found.");
            }

            var newTier = await _context.MembershipTiers.FindAsync(newMembershipTierId);
            if (newTier == null)
            {
                _logger.LogWarning("Attempt to assign non-existent membership tier ID: {NewTierId} to company {CompanyId}", newMembershipTierId, companyId);
                return (false, "Membership tier not found.");
            }

            if (company.MembershipTierId == newMembershipTierId)
            {
                return (false, "You are already on this membership tier.");
            }

            company.MembershipTierId = newMembershipTierId;
            company.Updated = DateTime.UtcNow; // Update timestamp

            try
            {
                var result = await _userManager.UpdateAsync(company);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Company {CompanyId} successfully updated to tier {TierName}", companyId, newTier.Name);
                    return (true, $"Membership updated to {newTier.Name} successfully!");
                }
                else
                {
                    _logger.LogError("Failed to update company {CompanyId} membership: {Errors}", companyId, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return (false, "Failed to update membership. Please try again.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating company {CompanyId} membership to tier {TierName}", companyId, newTier.Name);
                return (false, "An error occurred while updating your membership.");
            }
        }
    }
}