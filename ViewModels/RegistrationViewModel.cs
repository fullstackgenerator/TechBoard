using TechBoard.ViewModels.Company.Account;
using TechBoard.ViewModels.User.Account;

namespace TechBoard.ViewModels
{
    public class RegistrationViewModel
    {
        public UserRegisterViewModel User { get; set; } = new UserRegisterViewModel();
        public CompanyRegisterViewModel Company { get; set; } = new CompanyRegisterViewModel();
        
        public bool IsCompanyRegistration { get; set; }
    }
}