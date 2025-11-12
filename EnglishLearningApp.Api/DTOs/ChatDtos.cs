namespace EnglishLearningApp.Api.DTOs;

public class CreateChatSessionRequestDto
{
    public string? Title { get; set; }
}

public class SendMessageRequestDto
{
    public string Message { get; set; } = string.Empty;
}

public class ChatSessionDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int MessageCount { get; set; }
}

public class ChatMessageDto
{
    public string Id { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
