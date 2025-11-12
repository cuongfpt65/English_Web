using EnglishLearningApp.Data.Entities.User;

namespace EnglishLearningApp.Data.Entities.Chatbot
{
    public class ChatSession
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual AppUser User { get; set; }
        public virtual ICollection<ChatMessage> Messages { get; set; }
    }

    public class ChatMessage
    {
        public Guid Id { get; set; }
        public Guid ChatSessionId { get; set; }
        public string Sender { get; set; } = "User"; // User or Bot
        public string Message { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual ChatSession ChatSession { get; set; }
    }

    public class Vocabulary
    {
        public Guid Id { get; set; }
        public string Word { get; set; } = "";
        public string Meaning { get; set; } = "";
        public string? Example { get; set; }
        public string? ImageUrl { get; set; }
        public string? Topic { get; set; }
        public string? Level { get; set; }

        public virtual ICollection<UserVocabulary> UserVocabularies { get; set; }
    }

    public class UserVocabulary
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid VocabularyId { get; set; }
        public bool IsLearned { get; set; } = false;
        public string? Note { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        public virtual AppUser User { get; set; }
        public virtual Vocabulary Vocabulary { get; set; }
    }

    public class GrammarNote
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Example { get; set; } = "";
        public string? Level { get; set; }
    }

    public class TranslationHistory
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string OriginalText { get; set; } = "";
        public string TranslatedText { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual AppUser User { get; set; }
    }
}
