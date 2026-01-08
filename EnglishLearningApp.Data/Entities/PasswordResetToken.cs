namespace EnglishLearningApp.Data.Entities.User
{
    public class PasswordResetToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Email { get; set; } = null!;
        public string ResetCode { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        
        public virtual AppUser User { get; set; } = null!;
    }
}
