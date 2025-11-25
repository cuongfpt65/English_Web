using System.ComponentModel.DataAnnotations;

namespace EnglishLearningApp.Api.DTOs;

public class LoginRequestDto
{
    [Required]
    public string EmailOrPhone { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequestDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "Mật khẩu phải chứa ít nhất 1 chữ hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt")]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    [Compare("Password", ErrorMessage = "Xác nhận mật khẩu không khớp")]
    public string ConfirmPassword { get; set; } = string.Empty;
    
    public string? PhoneNumber { get; set; }
    
    [Required]
    public string Role { get; set; } = "Student"; // Student or Teacher
}

public class PhoneAuthRequestDto
{
    [Required]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [Required]
    public string VerificationCode { get; set; } = string.Empty;
    
    public bool CreateAccount { get; set; }
    
    public string? Name { get; set; }
    
    public string? Email { get; set; }
}

public class SendCodeRequestDto
{
    [Required]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [Required]
    public string Type { get; set; } = string.Empty; // "Login" or "Register"
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    
    public UserDto User { get; set; } = new();
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public string? PhoneNumber { get; set; }
      public string Role { get; set; } = "Student";
    
    public string Status { get; set; } = "Active";
    
    public DateTime CreatedAt { get; set; }
}
