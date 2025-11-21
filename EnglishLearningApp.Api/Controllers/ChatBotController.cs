using ERSP.Api.Services;
using Microsoft.AspNetCore.Mvc;

public class ChatRequest
{
    public String Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class ChatResponse
{
    public string Answer { get; set; } = string.Empty;
}

[ApiController]
[Route("api/[controller]")]
public class ChatBotController : ControllerBase
{
    private readonly ChatBotService _chatbotService;

    public ChatBotController(ChatBotService chatService)
    {
        _chatbotService = chatService;
    }

    [HttpPost]
    public async Task<ActionResult<ChatResponse>> Post([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message is required");

        var answer = await _chatbotService.HandleAsync(request.Message, request.Type);

        return new ChatResponse
        {
            Answer = answer
        };
    }
}