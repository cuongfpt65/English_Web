using EnglishLearningApp.Data.Entities.User;

namespace EnglishLearningApp.Repository.Interfaces
{
    public interface IUserRepository
    {
        Task<AppUser?> GetByEmailAsync(string email);
        Task<AppUser?> GetByPhoneAsync(string phoneNumber);
        Task<AppUser?> GetByIdAsync(Guid id);
        Task<AppUser> CreateAsync(AppUser user);
        Task<AppUser> UpdateAsync(AppUser user);
        Task<bool> ExistsAsync(string email, string? phoneNumber = null);
        
        // Password Reset methods
        Task<PasswordResetToken> CreateResetTokenAsync(PasswordResetToken token);
        Task<PasswordResetToken?> GetResetTokenAsync(string email, string code);
        Task<bool> MarkTokenAsUsedAsync(Guid tokenId);
        Task DeleteExpiredTokensAsync(string email);
    }
}
