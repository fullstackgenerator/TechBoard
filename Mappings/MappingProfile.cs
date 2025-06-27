using AutoMapper;
using TechBoard.Models.Domain;
using TechBoard.ViewModels.Company.Account;
using TechBoard.ViewModels.Company.Dashboard;
using TechBoard.ViewModels.Company.Profile;
using TechBoard.ViewModels.JobPost;

namespace TechBoard.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // company <=> ViewModels
        CreateMap<Company, CompanyProfileViewModel>().ReverseMap();
        CreateMap<Company, EditCompanyProfileViewModel>().ReverseMap();
        CreateMap<Company, CompanyRegisterViewModel>().ReverseMap();
        CreateMap<Company, ChangeCompanyPasswordViewModel>().ReverseMap();
        CreateMap<Company, CompanyLoginViewModel>().ReverseMap();
        CreateMap<Company, CompanyDashboardViewModel>().ReverseMap();
        CreateMap<Company, CreateJobPostViewModel>().ReverseMap();
        CreateMap<Company, EditJobPostViewModel>().ReverseMap();
        CreateMap<JobPost, JobPostViewModel>()
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company.Name));
    }
}