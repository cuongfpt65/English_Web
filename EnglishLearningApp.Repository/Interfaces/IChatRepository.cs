using EnglishLearningApp.Data.Entities.Chatbot;

namespace EnglishLearningApp.Repository.Interfaces
{
    public interface IChatRepository
    {
        Task<ChatSession> CreateSessionAsync(Guid userId, string title);
        Task<ChatSession?> GetSessionByIdAsync(Guid sessionId);
        Task<List<ChatSession>> GetUserSessionsAsync(Guid userId);
        Task<ChatMessage> AddMessageAsync(Guid sessionId, string sender, string message);
        Task<List<ChatMessage>> GetSessionMessagesAsync(Guid sessionId);
        Task UpdateSessionTitleAsync(Guid sessionId, string title);
    }
}
