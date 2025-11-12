using Microsoft.AspNetCore.Mvc;
using EnglishLearningApp.Api.DTOs;
using EnglishLearningApp.Service.Interfaces;

namespace EnglishLearningApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                var result = await _authService.RegisterAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Registration failed", error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var result = await _authService.LoginAsync(request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Login failed", error = ex.Message });
            }
        }

        [HttpPost("send-code")]
        public async Task<IActionResult> SendVerificationCode([FromBody] SendCodeRequestDto request)
        {
            try
            {
                var code = await _authService.SendVerificationCodeAsync(request);
                return Ok(new { code }); // In production, don't return the code
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to send verification code", error = ex.Message });
            }
        }

        [HttpPost("phone-auth")]
        public async Task<IActionResult> PhoneAuth([FromBody] PhoneAuthRequestDto request)
        {
            try
            {
                var result = await _authService.LoginWithPhoneAsync(request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Phone authentication failed", error = ex.Message });
            }
        }
    }
}
