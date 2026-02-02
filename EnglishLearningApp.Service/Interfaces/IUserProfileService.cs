using Microsoft.AspNetCore.Http;

namespace EnglishLearningApp.Service.Interfaces;

public interface IUserProfileService
{
    Task<object> GetUserProfileAsync(Guid userId);
    Task<object> UpdateProfileAsync(Guid userId, string fullName, string? phoneNumber);
    Task<object> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    Task<object> UpdateAvatarAsync(Guid userId, IFormFile avatarFile);
}
