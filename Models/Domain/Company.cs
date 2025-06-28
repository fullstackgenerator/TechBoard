using Microsoft.AspNetCore.Identity;

namespace TechBoard.Models.Domain;

public class Company : IdentityUser
{
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Country { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string IdNumber { get; set; } = null!;
    
    // navigation property
    public ICollection<JobPost> JobPosts { get; set; } = new List<JobPost>();
}