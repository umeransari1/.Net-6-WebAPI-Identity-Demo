using AspNetIdentityDemo.Shared.Models;
using AspNetIdentityDemo.Shared.ViewModels;

namespace AspNetIdentityDemo.Api.Services.Interfaces
{
    public interface IUserService
    {
        Task<ResponseModel> RegisterUser(RegisterViewModel user);
        Task<ResponseModel> LoginUser(LoginViewModel user);
        Task<ResponseModel> ConfirmEmail(string userId, string token);
        Task<ResponseModel> ForgotPassword(string email);
        Task<ResponseModel> ResetPassword(ResetPasswordViewModel model);
    }
}
