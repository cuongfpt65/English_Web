using System.ComponentModel.DataAnnotations;

namespace EnglishLearningApp.Api.DTOs;

public class UpdateProfileRequestDto
{
    [Required(ErrorMessage = "Tên không được để trống")]
    [MinLength(2, ErrorMessage = "Tên phải có ít nhất 2 ký tự")]
    public string FullName { get; set; } = string.Empty;
    
    public string? PhoneNumber { get; set; }
}

public class ChangePasswordRequestDto
{
    [Required(ErrorMessage = "Mật khẩu hiện tại không được để trống")]
    public string CurrentPassword { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
    [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
        ErrorMessage = "Mật khẩu phải chứa ít nhất 1 chữ hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt")]
    public string NewPassword { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống")]
    [Compare("NewPassword", ErrorMessage = "Xác nhận mật khẩu không khớp")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

public class UpdateAvatarResponseDto
{
    public string AvatarUrl { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class UserProfileResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
