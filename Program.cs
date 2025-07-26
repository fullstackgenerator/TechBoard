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
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Cookie configuration
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Home/AccessDenied";
});

// Register repositories
builder.Services.AddScoped<IJobPostRepository, JobPostRepository>();
builder.Services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();

// Register services
builder.Services.AddScoped<IJobPostService, JobPostService>();
builder.Services.AddScoped<IJobApplicationService, JobApplicationService>();
builder.Services.AddScoped<IMembershipService, MembershipService>();
builder.Services.AddScoped<ICompanyAnalyticsService, CompanyAnalyticsService>();

// Email service registration
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IEmailService, MockEmailService>();
}
else
{
    // For production - uncomment when ready
    // builder.Services.AddScoped<IEmailService, RealEmailService>();
    builder.Services.AddScoped<IEmailService, MockEmailService>();
}

builder.Services.AddControllersWithViews();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddRazorPages();

// Configure Identity options for password reset tokens
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(24);
});

// Configure email settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

var app = builder.Build();

await InitializeDatabaseAsync(app.Services, builder.Configuration);

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

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

app.Run();

static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider, IConfiguration configuration)
{
    using var scope = serviceProvider.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");

        await CreateRolesAsync(roleManager, logger);
        
        await CreateAdminUserAsync(userManager, configuration, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the database.");
        throw;
    }
}

static async Task CreateRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
{
    var roles = new[] { Roles.Admin, Roles.Company, Roles.User };
    
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var result = await roleManager.CreateAsync(new IdentityRole(role));
            if (result.Succeeded)
            {
                logger.LogInformation("Role '{Role}' created successfully.", role);
            }
            else
            {
                logger.LogError("Failed to create role '{Role}': {Errors}", 
                    role, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            logger.LogInformation("Role '{Role}' already exists.", role);
        }
    }
}

static async Task CreateAdminUserAsync(UserManager<ApplicationUser> userManager, IConfiguration configuration, ILogger logger)
{
    var adminUsername = configuration["SeedData:AdminUser:Username"];
    var adminEmail = configuration["SeedData:AdminUser:Email"];
    var adminPassword = configuration["SeedData:AdminUser:Password"];

    if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
    {
        logger.LogWarning("Admin seed credentials not found in configuration. Skipping admin user creation.");
        return;
    }

    var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
    if (existingAdmin != null)
    {
        logger.LogInformation("Admin user already exists with email: {Email}", adminEmail);
        return;
    }
    
    var adminUser = new ApplicationUser
    {
        UserName = adminEmail,
        Email = adminEmail,
        Name = adminUsername ?? "Admin",
        EmailConfirmed = true,
        Created = DateTime.UtcNow,
        Updated = DateTime.UtcNow,
        IsBlocked = false
    };

    var createResult = await userManager.CreateAsync(adminUser, adminPassword);
    if (createResult.Succeeded)
    {
        var roleResult = await userManager.AddToRoleAsync(adminUser, Roles.Admin);
        if (roleResult.Succeeded)
        {
            logger.LogInformation("Admin user created successfully with email: {Email}", adminEmail);
        }
        else
        {
            logger.LogError("Failed to assign admin role to user: {Errors}", 
                string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }
    }
    else
    {
        logger.LogError("Failed to create admin user: {Errors}", 
            string.Join(", ", createResult.Errors.Select(e => e.Description)));
    }
}