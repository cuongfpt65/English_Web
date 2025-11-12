using EnglishLearningApp.Data.Entities.User;
using EnglishLearningApp.Data.Entities.Chatbot;

namespace EnglishLearningApp.Data.Entities.Class
{
    public class ClassRoom
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public Guid TeacherId { get; set; }
        public string InviteCode { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual AppUser Teacher { get; set; }
        public virtual ICollection<ClassMember> Members { get; set; }
        public virtual ICollection<ClassMessage> Messages { get; set; }
    }

    public class ClassMember
    {
        public Guid Id { get; set; }
        public Guid ClassRoomId { get; set; }
        public Guid UserId { get; set; }
        public string Role { get; set; } = "Student";
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public virtual ClassRoom ClassRoom { get; set; }
        public virtual AppUser User { get; set; }
    }

    public class ClassMessage
    {
        public Guid Id { get; set; }
        public Guid ClassRoomId { get; set; }
        public Guid SenderId { get; set; }
        public string Message { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ClassRoom ClassRoom { get; set; }
        public virtual AppUser Sender { get; set; }
    }

    public class ClassQuiz
    {
        public Guid Id { get; set; }
        public Guid ClassRoomId { get; set; }
        public Guid CreatedById { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }
        public bool IsActive { get; set; } = true;
        public int TimeLimit { get; set; } // in minutes

        public virtual ClassRoom ClassRoom { get; set; }
        public virtual AppUser CreatedBy { get; set; }
        public virtual ICollection<ClassQuizQuestion> Questions { get; set; }
        public virtual ICollection<ClassQuizAttempt> Attempts { get; set; }
    }

    public class ClassQuizQuestion
    {
        public Guid Id { get; set; }
        public Guid QuizId { get; set; }
        public Guid VocabularyId { get; set; }
        public string QuestionType { get; set; } = "MultipleChoice"; // MultipleChoice, FillBlank, Translation
        public string QuestionText { get; set; } = "";
        public string CorrectAnswer { get; set; } = "";
        public string[]? Options { get; set; } // For multiple choice
        public int Order { get; set; }

        public virtual ClassQuiz Quiz { get; set; }
        public virtual Vocabulary Vocabulary { get; set; }
    }

    public class ClassQuizAttempt
    {
        public Guid Id { get; set; }
        public Guid QuizId { get; set; }
        public Guid UserId { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public bool IsCompleted { get; set; }

        public virtual ClassQuiz Quiz { get; set; }
        public virtual AppUser User { get; set; }
        public virtual ICollection<ClassQuizAnswer> Answers { get; set; }
    }

    public class ClassQuizAnswer
    {
        public Guid Id { get; set; }
        public Guid AttemptId { get; set; }
        public Guid QuestionId { get; set; }
        public string UserAnswer { get; set; } = "";
        public bool IsCorrect { get; set; }

        public virtual ClassQuizAttempt Attempt { get; set; }
        public virtual ClassQuizQuestion Question { get; set; }
    }

    public class ClassMemberStats
    {
        public Guid Id { get; set; }
        public Guid ClassRoomId { get; set; }
        public Guid UserId { get; set; }
        public int VocabulariesLearned { get; set; }
        public int QuizzesCompleted { get; set; }
        public double AverageQuizScore { get; set; }
        public int TotalPoints { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public virtual ClassRoom ClassRoom { get; set; }
        public virtual AppUser User { get; set; }
    }

    public class ClassAchievement
    {
        public Guid Id { get; set; }
        public Guid ClassRoomId { get; set; }
        public Guid UserId { get; set; }
        public string AchievementType { get; set; } = ""; // TopScorer, VocabularyMaster, QuizChampion, etc.
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime EarnedAt { get; set; } = DateTime.UtcNow;

        public virtual ClassRoom ClassRoom { get; set; }
        public virtual AppUser User { get; set; }
    }
}
