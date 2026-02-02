using EnglishLearningApp.Data;
using EnglishLearningApp.Data.Entities.User;
using EnglishLearningApp.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningApp.Service.Implementations;

public class UserProfileService : IUserProfileService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    private readonly IFileStorageService _fileStorageService;

    public UserProfileService(
        AppDbContext context,
        IPasswordHasher<AppUser> passwordHasher,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _fileStorageService = fileStorageService;
    }

    public async Task<object> GetUserProfileAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        
        if (user == null)
        {
            throw new InvalidOperationException("Người dùng không tồn tại");
        }

        return new
        {
            id = user.Id.ToString(),
            fullName = user.FullName,
            email = user.Email,
            phoneNumber = user.PhoneNumber,
            avatarUrl = user.AvatarUrl,
            role = user.Role,
            createdAt = user.CreatedAt,
            updatedAt = user.UpdatedAt
        };
    }

    public async Task<object> UpdateProfileAsync(Guid userId, string fullName, string? phoneNumber)
    {
        var user = await _context.Users.FindAsync(userId);
        
        if (user == null)
        {
            throw new InvalidOperationException("Người dùng không tồn tại");
        }

        user.FullName = fullName;
        user.PhoneNumber = phoneNumber;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new
        {
            message = "Cập nhật thông tin thành công",
            user = new
            {
                id = user.Id.ToString(),
                fullName = user.FullName,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                avatarUrl = user.AvatarUrl,
                role = user.Role,
                updatedAt = user.UpdatedAt
            }
        };
    }

    public async Task<object> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        
        if (user == null)
        {
            throw new InvalidOperationException("Người dùng không tồn tại");
        }

        // Verify current password
        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
        
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Mật khẩu hiện tại không đúng");
        }

        // Hash new password
        user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new
        {
            message = "Đổi mật khẩu thành công"
        };
    }

    public async Task<object> UpdateAvatarAsync(Guid userId, IFormFile avatarFile)
    {
        var user = await _context.Users.FindAsync(userId);
        
        if (user == null)
        {
            throw new InvalidOperationException("Người dùng không tồn tại");
        }

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var fileExtension = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(fileExtension))
        {
            throw new InvalidOperationException("Chỉ chấp nhận file ảnh định dạng: JPG, JPEG, PNG, GIF, WEBP");
        }

        // Validate file size (max 5MB)
        if (avatarFile.Length > 5 * 1024 * 1024)
        {
            throw new InvalidOperationException("Kích thước file không được vượt quá 5MB");
        }

        // Delete old avatar if exists
        if (!string.IsNullOrEmpty(user.AvatarUrl))
        {
            try
            {
                var oldPublicId = ExtractPublicIdFromUrl(user.AvatarUrl);
                if (!string.IsNullOrEmpty(oldPublicId))
                {
                    await _fileStorageService.DeleteFileAsync(oldPublicId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete old avatar: {ex.Message}");
                // Continue even if deletion fails
            }
        }

        // Upload new avatar to Cloudinary
        using var stream = avatarFile.OpenReadStream();
        var (avatarUrl, publicId) = await _fileStorageService.UploadFileAsync(
            stream, 
            avatarFile.FileName, 
            folder: "avatars"
        );

        // Update user avatar URL
        user.AvatarUrl = avatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new
        {
            message = "Cập nhật avatar thành công",
            avatarUrl = user.AvatarUrl
        };
    }

    private string? ExtractPublicIdFromUrl(string url)
    {
        try
        {
            // Cloudinary URL format: https://res.cloudinary.com/[cloud-name]/image/upload/v[version]/[public-id]
            // or: https://res.cloudinary.com/[cloud-name]/raw/upload/[public-id]
            var uri = new Uri(url);
            var segments = uri.AbsolutePath.Split('/');
            
            // Find "upload" segment and get everything after it
            var uploadIndex = Array.FindIndex(segments, s => s == "upload");
            if (uploadIndex >= 0 && uploadIndex < segments.Length - 1)
            {
                // Join all segments after "upload", skipping version if present
                var afterUpload = segments.Skip(uploadIndex + 1).ToList();
                
                // Remove version segment if present (starts with 'v' followed by numbers)
                if (afterUpload.Count > 0 && afterUpload[0].StartsWith("v") && afterUpload[0].Length > 1)
                {
                    afterUpload = afterUpload.Skip(1).ToList();
                }
                
                // Join remaining segments and remove file extension
                var publicIdWithExtension = string.Join("/", afterUpload);
                var publicId = Path.GetFileNameWithoutExtension(publicIdWithExtension);
                var folder = afterUpload.Count > 1 ? string.Join("/", afterUpload.Take(afterUpload.Count - 1)) : string.Empty;
                
                return string.IsNullOrEmpty(folder) ? publicId : $"{folder}/{publicId}";
            }
            
            return null;
        }
        catch
        {
            return null;
        }
    }
}
