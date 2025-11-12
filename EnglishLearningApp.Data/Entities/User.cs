using EnglishLearningApp.Data.Entities.Chatbot;
using EnglishLearningApp.Data.Entities.Class;
using EnglishLearningApp.Data.Entities.Game;
using EnglishLearningApp.Data.Entities.Test;

namespace EnglishLearningApp.Data.Entities.User
{    public class AppUser
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; } = false;
        public string PasswordHash { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public string Role { get; set; } = "Student";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<UserVocabulary> UserVocabularies { get; set; }
        public virtual ICollection<ChatSession> ChatSessions { get; set; }
        public virtual ICollection<UserGameScore> GameScores { get; set; }
        public virtual ICollection<ClassMember> ClassMembers { get; set; }
        public virtual ICollection<TestResult> TestResults { get; set; }
    }    public class UserGoogleLogin
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string GoogleId { get; set; } = null!;
        public string Email { get; set; } = null!;
        public virtual AppUser User { get; set; }
    }

    public class PhoneVerification
    {
        public Guid Id { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string VerificationCode { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public string Purpose { get; set; } = "Login"; // Login, Registration, PasswordReset
    }
}
