using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechBoard.Models.Domain;

namespace TechBoard.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Company> Companies { get; set; }
    public DbSet<JobPost> JobPosts { get; set; }
    public new DbSet<User> Users { get; set; }
    public DbSet<JobApplication> JobApplications { get; set; }
    public DbSet<MembershipTier> MembershipTiers { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Company
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasOne(c => c.MembershipTier)
                  .WithMany(m => m.Companies)
                  .HasForeignKey(c => c.MembershipTierId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(c => c.JobPosts)
                  .WithOne(j => j.Company)
                  .HasForeignKey(j => j.CompanyId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure JobPost
        modelBuilder.Entity<JobPost>(entity =>
        {
            entity.HasKey(j => j.Id);
            entity.Property(j => j.Title).IsRequired().HasMaxLength(200);
            entity.Property(j => j.Description).IsRequired();
            entity.Property(j => j.Requirements).IsRequired();
            entity.Property(j => j.Location).IsRequired().HasMaxLength(100);
            entity.Property(j => j.SalaryMin).HasColumnType("decimal(18,2)");
            entity.Property(j => j.SalaryMax).HasColumnType("decimal(18,2)");

            entity.HasMany(j => j.JobApplications)
                  .WithOne(a => a.JobPost)
                  .HasForeignKey(a => a.JobPostId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Configure JobApplication
        modelBuilder.Entity<JobApplication>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.CoverLetter).IsRequired(); 

            entity.HasOne(a => a.User)
                  .WithMany(u => u.JobApplications)
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(a => new { a.UserId, a.JobPostId })
                  .IsUnique(); // Prevent duplicate applications
        });

        // Configure MembershipTier
        modelBuilder.Entity<MembershipTier>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Name).IsRequired().HasMaxLength(50);
            entity.Property(m => m.Description).IsRequired();
            entity.Property(m => m.Price).HasColumnType("decimal(18,2)");
        });

        // Seed MembershipTiers
        modelBuilder.Entity<MembershipTier>().HasData(
            new MembershipTier
            {
                Id = 1,
                Name = "Basic",
                Description = "Perfect for startups and small companies",
                Price = 0,
                MaxJobPosts = 2,
                JobPostDurationDays = 30,
                CanPostFeatured = false,
                CanAccessAnalytics = false,
                CanContactCandidates = true,
                MaxApplicationsPerJob = 50,
                Created = new DateTime(2024, 1, 1),
                Updated = new DateTime(2024, 1, 1) 
            },
            new MembershipTier
            {
                Id = 2,
                Name = "Premium",
                Description = "Great for growing companies",
                Price = 99.99m,
                MaxJobPosts = 10,
                JobPostDurationDays = 60,
                CanPostFeatured = true,
                CanAccessAnalytics = true,
                CanContactCandidates = true,
                MaxApplicationsPerJob = 200,
                Created = new DateTime(2024, 1, 1),
                Updated = new DateTime(2024, 1, 1) 
            },
            new MembershipTier
            {
                Id = 3,
                Name = "Enterprise",
                Description = "For large enterprises with high volume recruiting",
                Price = 299.99m,
                MaxJobPosts = -1,
                JobPostDurationDays = 90,
                CanPostFeatured = true,
                CanAccessAnalytics = true,
                CanContactCandidates = true,
                MaxApplicationsPerJob = -1,
                Created = new DateTime(2024, 1, 1),
                Updated = new DateTime(2024, 1, 1) 
            }
        );
    }
}