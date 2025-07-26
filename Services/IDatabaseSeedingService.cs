using Microsoft.AspNetCore.Identity;
using TechBoard.Models.Domain;

namespace TechBoard.Services;

public interface IDatabaseSeedingService
{
    Task SeedRolesAsync();
    Task SeedAdminUserAsync();
    Task SeedDevelopmentDataAsync();
}