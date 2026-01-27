using ERSP.Api.Services;
using EnglishLearningApp.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class ChatRequest
{
    public String Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? SessionId { get; set; }
}

public class ChatResponse
{
    public string Answer { get; set; } = string.Empty;
    public Guid? SessionId { get; set; }
}

public class CreateSessionRequest
{
    public string Title { get; set; } = "New Chat";
}

public class SessionResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class MessageResponse
{
    public Guid Id { get; set; }
    public string Sender { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class ChatBotController : ControllerBase
{
    private readonly ChatBotService _chatbotService;
    private readonly IChatRepository _chatRepository;

    public ChatBotController(ChatBotService chatService, IChatRepository chatRepository)
    {
        _chatbotService = chatService;
        _chatRepository = chatRepository;
    }

    // Gửi tin nhắn chat
    [HttpPost]
    public async Task<ActionResult<ChatResponse>> Post([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message is required");

        var userId = GetUserId();
        Guid? sessionId = request.SessionId;

        // Nếu chưa có session, tạo session mới
        if (!sessionId.HasValue && userId.HasValue)
        {
            var session = await _chatRepository.CreateSessionAsync(
                userId.Value, 
                $"Chat {DateTime.Now:HH:mm dd/MM/yyyy}"
            );
            sessionId = session.Id;
        }

        var answer = await _chatbotService.HandleAsync(
            request.Message, 
            request.Type, 
            sessionId, 
            userId
        );

        return new ChatResponse
        {
            Answer = answer,
            SessionId = sessionId
        };
    }

    // Tạo session chat mới
    [HttpPost("sessions")]
    public async Task<ActionResult<SessionResponse>> CreateSession([FromBody] CreateSessionRequest request)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
            return Unauthorized();

        var session = await _chatRepository.CreateSessionAsync(userId.Value, request.Title);

        return new SessionResponse
        {
            Id = session.Id,
            Title = session.Title,
            CreatedAt = session.CreatedAt
        };
    }

    // Lấy danh sách các session của user
    [HttpGet("sessions")]
    public async Task<ActionResult<List<SessionResponse>>> GetSessions()
    {
        var userId = GetUserId();
        if (!userId.HasValue)
            return Unauthorized();

        var sessions = await _chatRepository.GetUserSessionsAsync(userId.Value);

        return sessions.Select(s => new SessionResponse
        {
            Id = s.Id,
            Title = s.Title,
            CreatedAt = s.CreatedAt
        }).ToList();
    }

    // Lấy lịch sử chat của một session
    [HttpGet("sessions/{sessionId}/messages")]
    public async Task<ActionResult<List<MessageResponse>>> GetSessionMessages(Guid sessionId)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
            return Unauthorized();

        var session = await _chatRepository.GetSessionByIdAsync(sessionId);
        if (session == null)
            return NotFound();

        if (session.UserId != userId.Value)
            return Forbid();

        var messages = await _chatRepository.GetSessionMessagesAsync(sessionId);

        return messages.Select(m => new MessageResponse
        {
            Id = m.Id,
            Sender = m.Sender,
            Message = m.Message,
            CreatedAt = m.CreatedAt
        }).ToList();
    }

    // Cập nhật tên session
    [HttpPut("sessions/{sessionId}")]
    public async Task<IActionResult> UpdateSessionTitle(Guid sessionId, [FromBody] CreateSessionRequest request)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
            return Unauthorized();

        var session = await _chatRepository.GetSessionByIdAsync(sessionId);
        if (session == null)
            return NotFound();

        if (session.UserId != userId.Value)
            return Forbid();

        await _chatRepository.UpdateSessionTitleAsync(sessionId, request.Title);

        return Ok();
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }
}