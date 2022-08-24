using AspNetIdentityDemo.Api.Services.Interfaces;
using AspNetIdentityDemo.Shared.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AspNetIdentityDemo.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private IUserService _userService { get; set; }
        private IConfiguration _configuration { get; set; }

        public AuthController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.RegisterUser(model);

                if (result.IsSuccess)
                    return Ok(result);

                return BadRequest(result);
            }

            return BadRequest("Some Properties are not valid");
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.LoginUser(model);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }

            return BadRequest("Some Properties are not valid");
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token) 
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return NotFound();

            var result = await _userService.ConfirmEmail(userId, token);

            if (result.IsSuccess)
            {
                return Redirect($"{_configuration["AppURL"]}/ConfirmEmail.html");
            }

            return BadRequest(result);
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if(string.IsNullOrEmpty(email))
                return NotFound();

            var result = await _userService.ForgotPassword(email);

            if(result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.ResetPassword(model);

                if (result.IsSuccess)
                    return Ok(result);
            }
            return BadRequest("Some Properties are not valid");
        }
    }
}
