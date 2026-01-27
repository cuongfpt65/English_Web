using EnglishLearningApp.Data;
using EnglishLearningApp.Data.Entities.Chatbot;
using EnglishLearningApp.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningApp.Repository.Implementations
{
    public class ChatRepository : IChatRepository
    {
        private readonly AppDbContext _context;

        public ChatRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ChatSession> CreateSessionAsync(Guid userId, string title)
        {
            var session = new ChatSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                CreatedAt = DateTime.UtcNow
            };

            _context.ChatSessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<ChatSession?> GetSessionByIdAsync(Guid sessionId)
        {
            return await _context.ChatSessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.Id == sessionId);
        }

        public async Task<List<ChatSession>> GetUserSessionsAsync(Guid userId)
        {
            return await _context.ChatSessions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<ChatMessage> AddMessageAsync(Guid sessionId, string sender, string message)
        {
            var chatMessage = new ChatMessage
            {
                Id = Guid.NewGuid(),
                ChatSessionId = sessionId,
                Sender = sender,
                Message = message,
                CreatedAt = DateTime.UtcNow
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();
            return chatMessage;
        }

        public async Task<List<ChatMessage>> GetSessionMessagesAsync(Guid sessionId)
        {
            return await _context.ChatMessages
                .Where(m => m.ChatSessionId == sessionId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateSessionTitleAsync(Guid sessionId, string title)
        {
            var session = await _context.ChatSessions.FindAsync(sessionId);
            if (session != null)
            {
                session.Title = title;
                await _context.SaveChangesAsync();
            }
        }
    }
}
