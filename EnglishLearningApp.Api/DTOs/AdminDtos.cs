using System.ComponentModel.DataAnnotations;

namespace EnglishLearningApp.Api.DTOs;

public class TeacherApprovalRequestDto
{
    [Required]
    public string FullName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    public string? PhoneNumber { get; set; }
    
    [Required]
    public string Qualification { get; set; } = string.Empty;
    
    [Required]
    public string Experience { get; set; } = string.Empty;
    
    public string? CertificateUrl { get; set; }
}

public class ApprovalActionDto
{
    [Required]
    public Guid ApprovalId { get; set; }
    
    public string? RejectionReason { get; set; }
}

public class ChangeUserRoleDto
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public string Role { get; set; } = string.Empty;
}

public class ToggleUserStatusDto
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public bool IsActive { get; set; }
}

public class RejectTeacherDto
{
    [Required]
    public string Reason { get; set; } = string.Empty;
}

public class PendingTeacherDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}
