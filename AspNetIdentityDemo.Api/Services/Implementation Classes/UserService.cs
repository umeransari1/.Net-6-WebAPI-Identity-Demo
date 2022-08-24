using AspNetIdentityDemo.Api.Services.Interfaces;
using AspNetIdentityDemo.Shared.Models;
using AspNetIdentityDemo.Shared.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AspNetIdentityDemo.Api.Services.Implementation_Classes
{
    public class UserService : IUserService
    {
        private UserManager<IdentityUser> _userManager { get; set; }
        private IMailService _mailService { get; set; }
        private IConfiguration _configuration { get; set; }

        public UserService(UserManager<IdentityUser> userManager, IConfiguration configuration, IMailService mailService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _mailService = mailService;
        }

        public async Task<ResponseModel> RegisterUser(RegisterViewModel user)
        {
            if(user == null)
                throw new NullReferenceException("User is null");

            if (user.Password != user.ConfirmPassword)
                return new ResponseModel
                {
                    Message = "Password doesn't match",
                    IsSuccess = false
                };

            var IdentityUser = new IdentityUser
            {
                Email = user.Email,
                UserName = user.Email
            };

            var result = await _userManager.CreateAsync(IdentityUser, user.Password);

            if (result.Succeeded)
            {
                var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(IdentityUser);

                var encodedEmailToken = Encoding.UTF8.GetBytes(confirmationToken);
                var validEmailToken = WebEncoders.Base64UrlEncode(encodedEmailToken);

                string url = $"{_configuration["AppURL"]}/api/auth/ConfirmEmail?userId={IdentityUser.Id}&token={validEmailToken}";

                string content = $"You're on your way!<br/><br/>Let's confirm your email address.<br/><br/>"+
                    "By clicking on the following link, you are confirming your email address.<br/><br/> <a href='{url}'>Confirm Email Address</a>";

                await _mailService.SendVerificationEmailAsync(IdentityUser.Email, "Welcome to Auth Demo Application! Confirm your Email", content);

                return new ResponseModel
                {
                    Message = "User Created Successfully",
                    IsSuccess = true
                };
            }

            return new ResponseModel
            {
                Message = "User didn't Created",
                IsSuccess = false,
                Errors = result.Errors.Select(e => e.Description)
            };
        }

        public async Task<ResponseModel> LoginUser(LoginViewModel model)
        {
            if (model == null)
                throw new NullReferenceException("Email/Password are required !!");

            var user = await _userManager.FindByEmailAsync(model.Email);

            if(user == null)
                return new ResponseModel
                {
                    Message = "Invalid Email",
                    IsSuccess = false
                };

            var result = await _userManager.CheckPasswordAsync(user, model.Password);

            if(!result)
                return new ResponseModel
                {
                    Message = "Invalid Password",
                    IsSuccess = false
                };

            var claims = new[]
            {
                new Claim("Email", model.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var keyBuffer = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AuthSettings:IssuerSigningKey"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["AuthSettings:ValidIssuer"],
                audience: _configuration["AuthSettings:ValidAudience"],
                claims: claims,
                expires: DateTime.Now.AddDays(2),
                signingCredentials: new SigningCredentials(keyBuffer, SecurityAlgorithms.HmacSha256)
            );

            string tokenAsString = new JwtSecurityTokenHandler().WriteToken(token);

            return new ResponseModel { 
                Message = tokenAsString,
                IsSuccess = true,
                ExpireDate = token.ValidTo
            };
        }

        public async Task<ResponseModel> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "User Not Found"
                };

            var decodedToken = WebEncoders.Base64UrlDecode(token);
            var normalToken = Encoding.UTF8.GetString(decodedToken);

            var result = await _userManager.ConfirmEmailAsync(user, normalToken);

            if (result.Succeeded)
            {
                string content = $"<strong>Hello,</strong><br/><br/>" +
                    "Your email has been confirmed successfully, Now you are part JWT Auth Demo family.<br/><br/>" +
                    "Thanks,<br/>JWT Auth Demo Team";

                await _mailService.SendVerificationEmailAsync(user.Email, "Your Email confirmed successfully", content);

                return new ResponseModel
                {
                    IsSuccess = true,
                    Message = "Email Confirmed Successfully"
                };
            }

            return new ResponseModel
            {
                IsSuccess = false,
                Message = "Email Doesn't Confirm",
                Errors = result.Errors.Select(e => e.Description)
            };
        }

        public async Task<ResponseModel> ForgotPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "No User exist with this email"
                };

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedEmailToken = Encoding.UTF8.GetBytes(token);
            var validEmailToken = WebEncoders.Base64UrlEncode(encodedEmailToken);

            string url = $"{_configuration["AppURL"]}/ResetPassword?email={email}&token={validEmailToken}";

            string content = $"<strong>Hello,</strong><br/><br/>"+
                "JWT Auth Demo recently received a request for a forgotten password.<br/><br/>"+
                "To Change your passowrd, please click on below link.<br/><br/> <a href='{url}'>Reset your password</a><br/>If you didn't request this change, you do not need to do anything.<br/><br/>"+
                "Thanks,<br/>JWT Auth Demo Team";
            await _mailService.SendVerificationEmailAsync(email, "Forgotten password request", content);

            return new ResponseModel
            {
                IsSuccess = true,
                Message = "Reset password Url has been sent to the email successfully!"
            };
        }

        public async Task<ResponseModel> ResetPassword(ResetPasswordViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "No User exist with this email"
                };

            if (model.Password != model.ConfirmPassword)
                return new ResponseModel
                {
                    Message = "Password doesn't match",
                    IsSuccess = false
                };

            var decodedToken = WebEncoders.Base64UrlDecode(model.Token);
            var normalToken = Encoding.UTF8.GetString(decodedToken);

            var result = await _userManager.ResetPasswordAsync(user, normalToken, model.Password);

            if (result.Succeeded)
            {

                string content = $"<strong>Hello,</strong><br/><br/>" +
                    "You've successfully changed your JWT Auth Demo password.<br/><br/>" +
                    "Thanks,<br/>JWT Auth Demo Team";

                await _mailService.SendVerificationEmailAsync(model.Email, "Your password was successfully reset", content);

                return new ResponseModel
                {
                    Message = "Password reset successfully",
                    IsSuccess = true
                };
            }

            return new ResponseModel
            {
                Message = "Unfortunately, Something went Wrong",
                IsSuccess = false,
                Errors = result.Errors.Select(e => e.Description)
            };
        }
    }
}
