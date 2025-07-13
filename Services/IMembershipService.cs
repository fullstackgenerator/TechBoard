using TechBoard.Models.Domain;

namespace TechBoard.Services
{
    public interface IMembershipService
    {
        Task<IEnumerable<MembershipTier>> GetAllMembershipTiersAsync();
        Task<MembershipTier?> GetMembershipTierByIdAsync(int id);
        Task<MembershipTier?> GetMembershipTierByNameAsync(string name);
        Task<(bool Success, string Message)> UpdateCompanyMembershipAsync(string companyId, int newMembershipTierId);
    }
}