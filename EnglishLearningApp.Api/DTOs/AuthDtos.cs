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
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
    
    public string? PhoneNumber { get; set; }
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
    
    public DateTime CreatedAt { get; set; }
}
