using Microsoft.AspNetCore.Identity;
using TechBoard.Constants;
using TechBoard.Models.Domain;

namespace TechBoard.Data
{
    public static class ApplicationDbInitializer
    {
        public static async Task SeedRolesAndAdminUserAsync(IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roleNames = { Roles.Admin, Roles.Company, Roles.User };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var adminEmail = configuration["AdminUser:Email"];
            var adminPassword = configuration["AdminUser:Password"];

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            {
                Console.WriteLine(
                    "AdminUser:Email or AdminUser:Password not found in configuration. Skipping admin user creation.");
                return;
            }

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Name = "Super Admin",
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, Roles.Admin);
                    Console.WriteLine($"Admin user '{adminUser.Email}' created and assigned 'Admin' role.");
                }
                else
                {
                    Console.WriteLine(
                        $"Failed to create admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(adminUser, Roles.Admin))
                {
                    await userManager.AddToRoleAsync(adminUser, Roles.Admin);
                    Console.WriteLine(
                        $"Admin user '{adminUser.Email}' already exists, ensuring 'Admin' role is assigned.");
                }
                else
                {
                    Console.WriteLine($"Admin user '{adminUser.Email}' already exists and has 'Admin' role.");
                }
            }
        }
    }
}