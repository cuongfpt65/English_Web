using EnglishLearningApp.Data.Entities.Chatbot;
using EnglishLearningApp.Repository.Implementations;
using EnglishLearningApp.Service.Interfaces;

namespace EnglishLearningApp.Service.Implementations;

public class ChatService : IChatService
{
    private readonly IChatSessionRepository _sessionRepository;
    private readonly IChatMessageRepository _messageRepository;

    public ChatService(
        IChatSessionRepository sessionRepository,
        IChatMessageRepository messageRepository)
    {
        _sessionRepository = sessionRepository;
        _messageRepository = messageRepository;
    }

    public async Task<object> GetUserSessionsAsync(Guid userId)
    {
        var sessions = await _sessionRepository.GetUserSessionsAsync(userId);
        
        return sessions.Select(s => new
        {
            Id = s.Id.ToString(),
            s.Title,
            s.CreatedAt,
            MessageCount = s.Messages.Count
        });
    }

    public async Task<object> CreateSessionAsync(Guid userId, string title)
    {
        var session = new ChatSession
        {
            UserId = userId,
            Title = title ?? "New Conversation"
        };

        var created = await _sessionRepository.CreateAsync(session);

        return new
        {
            Id = created.Id.ToString(),
            created.Title,
            created.CreatedAt
        };
    }

    public async Task<object> GetSessionMessagesAsync(Guid sessionId)
    {
        var messages = await _messageRepository.GetSessionMessagesAsync(sessionId);
        
        return messages.Select(m => new
        {
            Id = m.Id.ToString(),
            m.Sender,
            m.Message,
            m.CreatedAt
        });
    }

    public async Task<object> SendMessageAsync(Guid userId, Guid sessionId, string message)
    {
        // Verify session belongs to user
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null || session.UserId != userId)
        {
            throw new UnauthorizedAccessException("Session not found or unauthorized");
        }

        // Save user message
        var userMessage = new ChatMessage
        {
            ChatSessionId = sessionId,
            Sender = "User",
            Message = message
        };
        var savedUserMessage = await _messageRepository.CreateAsync(userMessage);

        // Generate bot response (simplified - in real app, integrate with ChatGPT API)
        var botResponse = await GenerateBotResponseAsync(message);
        var botMessage = new ChatMessage
        {
            ChatSessionId = sessionId,
            Sender = "Bot",
            Message = botResponse
        };
        var savedBotMessage = await _messageRepository.CreateAsync(botMessage);

        return new
        {
            UserMessage = new
            {
                Id = savedUserMessage.Id.ToString(),
                savedUserMessage.Sender,
                savedUserMessage.Message,
                savedUserMessage.CreatedAt
            },
            BotMessage = new
            {
                Id = savedBotMessage.Id.ToString(),
                savedBotMessage.Sender,
                savedBotMessage.Message,
                savedBotMessage.CreatedAt
            }
        };
    }

    private async Task<string> GenerateBotResponseAsync(string userMessage)
    {
        // Mock AI response - in production, call OpenAI or similar service
        await Task.Delay(500);
        
        var responses = new[]
        {
            "That's a great question! Let me help you with that.",
            "I understand. Can you tell me more?",
            "Excellent! Keep practicing and you'll improve quickly.",
            "Let me explain that in a different way...",
            "That's correct! Well done!"
        };

        return responses[new Random().Next(responses.Length)];
    }
}
