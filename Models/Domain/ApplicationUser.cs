using Microsoft.AspNetCore.Identity;

namespace TechBoard.Models.Domain;

public abstract class ApplicationUser : IdentityUser
{
    public string Name { get; set; } = null!;
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime Updated { get; set; } = DateTime.UtcNow;
}