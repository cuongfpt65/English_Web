using EnglishLearningApp.Data;
using EnglishLearningApp.Data.Entities.Chatbot;
using EnglishLearningApp.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningApp.Repository.Implementations;

public interface IChatSessionRepository
{
    Task<IEnumerable<ChatSession>> GetUserSessionsAsync(Guid userId);
    Task<ChatSession?> GetByIdAsync(Guid id);
    Task<ChatSession> CreateAsync(ChatSession session);
    Task<bool> DeleteAsync(Guid id);
}

public interface IChatMessageRepository
{
    Task<IEnumerable<ChatMessage>> GetSessionMessagesAsync(Guid sessionId);
    Task<ChatMessage> CreateAsync(ChatMessage message);
}

public class ChatSessionRepository : IChatSessionRepository
{
    private readonly AppDbContext _context;

    public ChatSessionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ChatSession>> GetUserSessionsAsync(Guid userId)
    {
        return await _context.ChatSessions
            .Include(cs => cs.Messages)
            .Where(cs => cs.UserId == userId)
            .OrderByDescending(cs => cs.CreatedAt)
            .ToListAsync();
    }

    public async Task<ChatSession?> GetByIdAsync(Guid id)
    {
        return await _context.ChatSessions
            .Include(cs => cs.Messages)
            .FirstOrDefaultAsync(cs => cs.Id == id);
    }

    public async Task<ChatSession> CreateAsync(ChatSession session)
    {
        session.Id = Guid.NewGuid();
        session.CreatedAt = DateTime.UtcNow;
        _context.ChatSessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var session = await _context.ChatSessions.FindAsync(id);
        if (session == null) return false;

        _context.ChatSessions.Remove(session);
        await _context.SaveChangesAsync();
        return true;
    }
}

public class ChatMessageRepository : IChatMessageRepository
{
    private readonly AppDbContext _context;

    public ChatMessageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ChatMessage>> GetSessionMessagesAsync(Guid sessionId)
    {
        return await _context.ChatMessages
            .Where(cm => cm.ChatSessionId == sessionId)
            .OrderBy(cm => cm.CreatedAt)
            .ToListAsync();
    }

    public async Task<ChatMessage> CreateAsync(ChatMessage message)
    {
        message.Id = Guid.NewGuid();
        message.CreatedAt = DateTime.UtcNow;
        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }
}
