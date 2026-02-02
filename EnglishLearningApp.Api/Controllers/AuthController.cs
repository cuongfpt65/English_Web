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
        }        [HttpPost("register")]
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

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            try
            {
                await _authService.SendPasswordResetCodeAsync(request.Email);
                return Ok(new { message = "Nếu email tồn tại trong hệ thống, mã xác thực đã được gửi đến email của bạn" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Không thể gửi mã xác thực", error = ex.Message });
            }
        }

        [HttpPost("verify-reset-code")]
        public async Task<IActionResult> VerifyResetCode([FromBody] VerifyResetCodeRequestDto request)
        {
            try
            {
                var isValid = await _authService.VerifyResetCodeAsync(request.Email, request.Code);
                if (isValid)
                {
                    return Ok(new { message = "Mã xác thực hợp lệ", valid = true });
                }
                return BadRequest(new { message = "Mã xác thực không hợp lệ hoặc đã hết hạn", valid = false });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Không thể xác thực mã", error = ex.Message });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            try
            {
                await _authService.ResetPasswordAsync(request.Email, request.Code, request.NewPassword);
                return Ok(new { message = "Mật khẩu đã được đặt lại thành công" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Không thể đặt lại mật khẩu", error = ex.Message });
            }
        }
    }
}
