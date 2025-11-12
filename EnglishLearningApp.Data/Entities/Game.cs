using EnglishLearningApp.Data.Entities.User;

namespace EnglishLearningApp.Data.Entities.Game
{
    public class Game
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Type { get; set; } = ""; // Quiz, Flashcard, Matching...
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<GameQuestion> Questions { get; set; }
    }

    public class GameQuestion
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public string Question { get; set; } = "";
        public string AnswerA { get; set; } = "";
        public string AnswerB { get; set; } = "";
        public string AnswerC { get; set; } = "";
        public string AnswerD { get; set; } = "";
        public string CorrectAnswer { get; set; } = "";
        public virtual Game Game { get; set; }
    }

    public class UserGameScore
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid GameId { get; set; }
        public int Score { get; set; }
        public DateTime PlayedAt { get; set; } = DateTime.UtcNow;

        public virtual AppUser User { get; set; }
        public virtual Game Game { get; set; }
    }
}
