using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechBoard.Constants;
using TechBoard.Data;
using TechBoard.Mappings;
using TechBoard.Models.Domain;
using TechBoard.Repositories;
using TechBoard.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configure Identity with ApplicationUser as the base type
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => 
{
    options.SignIn.RequireConfirmedAccount = false; // Changed to false for development
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// cookie configuration
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/company/auth/login";
    options.LogoutPath = "/company/auth/logout";
    options.AccessDeniedPath = "/Home/AccessDenied";
});

// Register repositories
builder.Services.AddScoped<IJobPostRepository, JobPostRepository>();
builder.Services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();

// Register services
builder.Services.AddScoped<IJobPostService, JobPostService>();

builder.Services.AddControllersWithViews();
builder.Services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Database seeding logic
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var configuration = services.GetRequiredService<IConfiguration>();

        await context.Database.MigrateAsync();

        // Create roles if they don't exist
        var roles = new[] { Roles.Admin, Roles.Company, Roles.User };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                Console.WriteLine($"{role} role created.");
            }
        }

        // Seed admin user
        var adminUsername = configuration["SeedData:AdminUser:Username"];
        var adminPassword = configuration["SeedData:AdminUser:Password"];

        if (string.IsNullOrEmpty(adminUsername) || string.IsNullOrEmpty(adminPassword))
        {
            Console.WriteLine("WARNING: Admin seed credentials not found in User Secrets. Skipping admin user creation.");
        }
        else
        {
            if (await userManager.FindByNameAsync(adminUsername) == null)
            {
                // Create admin as a Company user with admin privileges
                var adminUser = new Company
                {
                    UserName = adminUsername,
                    Email = $"{adminUsername}@example.com",
                    EmailConfirmed = true, // Set to true for development
                    Name = "Admin Company",
                    Address = "Admin Address",
                    City = "Admin City",
                    Country = "Admin Country",
                    PostalCode = "00000",
                    Phone = "0000000000",
                    IdNumber = "ADMIN0001",
                    MembershipTierId = 3 // Enterprise tier for admin
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, Roles.Admin);
                    await userManager.AddToRoleAsync(adminUser, Roles.Company); // Also add Company role
                    Console.WriteLine("Admin user created successfully.");
                }
                else
                {
                    Console.WriteLine("Admin creation failed: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        // Seed test company user (optional for development)
        if (app.Environment.IsDevelopment())
        {
            var testCompanyEmail = "testcompany@example.com";
            if (await userManager.FindByEmailAsync(testCompanyEmail) == null)
            {
                var testCompany = new Company
                {
                    UserName = testCompanyEmail,
                    Email = testCompanyEmail,
                    EmailConfirmed = true,
                    Name = "Test Tech Company",
                    Address = "123 Tech Street",
                    City = "Tech City",
                    Country = "TechLand",
                    PostalCode = "12345",
                    Phone = "1234567890",
                    IdNumber = "TEST0001",
                    MembershipTierId = 1, // Basic tier
                    Website = "https://testtech.com",
                    Description = "A test technology company for development purposes.",
                    EmployeeCount = 50
                };

                var result = await userManager.CreateAsync(testCompany, "TestPass123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(testCompany, Roles.Company);
                    Console.WriteLine("Test company user created successfully.");
                }
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error seeding database.");
    }
}

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

app.Run();