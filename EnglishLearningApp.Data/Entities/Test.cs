using EnglishLearningApp.Data.Entities.User;

namespace EnglishLearningApp.Data.Entities.Test
{
    public class Test
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Level { get; set; } = "";
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual AppUser Creator { get; set; }
        public virtual ICollection<TestQuestion> Questions { get; set; }
    }

    public class TestQuestion
    {
        public Guid Id { get; set; }
        public Guid TestId { get; set; }
        public string Question { get; set; } = "";
        public string OptionA { get; set; } = "";
        public string OptionB { get; set; } = "";
        public string OptionC { get; set; } = "";
        public string OptionD { get; set; } = "";
        public string CorrectAnswer { get; set; } = "";

        public virtual Test Test { get; set; }
    }

    public class TestResult
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid TestId { get; set; }
        public double Score { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public virtual AppUser User { get; set; }
        public virtual Test Test { get; set; }
        public virtual ICollection<UserTestAnswer> Answers { get; set; }
    }

    public class UserTestAnswer
    {
        public Guid Id { get; set; }
        public Guid TestResultId { get; set; }
        public Guid QuestionId { get; set; }
        public string ChosenAnswer { get; set; } = "";
        public bool IsCorrect { get; set; }

        public virtual TestResult TestResult { get; set; }
        public virtual TestQuestion Question { get; set; }
    }
}
