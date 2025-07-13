using System.ComponentModel.DataAnnotations;

namespace TechBoard.ViewModels.Company.Membership
{
    public class CompanyMembershipViewModel
    {
        public int CurrentTierId { get; set; }

        [Display(Name = "Current Membership Tier")]
        public string CurrentTierName { get; set; } = null!;
        public string CurrentTierDescription { get; set; } = null!;
        public decimal CurrentTierPrice { get; set; }
        public int CurrentTierMaxJobPosts { get; set; }
        public int CurrentTierJobPostDurationDays { get; set; }
        public bool CurrentTierCanPostFeatured { get; set; }
        public bool CurrentTierCanAccessAnalytics { get; set; }
        public bool CurrentTierCanContactCandidates { get; set; }
        public int CurrentTierMaxApplicationsPerJob { get; set; }

        public IEnumerable<MembershipTierViewModel> AvailableTiers { get; set; } = new List<MembershipTierViewModel>();
    }

    public class MembershipTierViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public int MaxJobPosts { get; set; }
        public int JobPostDurationDays { get; set; }
        public bool CanPostFeatured { get; set; }
        public bool CanAccessAnalytics { get; set; }
        public bool CanContactCandidates { get; set; }
        public int MaxApplicationsPerJob { get; set; }
    }
}