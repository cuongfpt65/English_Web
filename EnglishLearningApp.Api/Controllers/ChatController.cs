using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EnglishLearningApp.Api.DTOs;
using EnglishLearningApp.Service.Interfaces;
using System.Security.Claims;

namespace EnglishLearningApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("sessions")]
        public async Task<IActionResult> GetChatSessions()
        {
            try
            {
                var userId = GetCurrentUserId();
                var sessions = await _chatService.GetUserSessionsAsync(userId);
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve chat sessions", error = ex.Message });
            }
        }

        [HttpPost("sessions")]
        public async Task<IActionResult> CreateChatSession([FromBody] CreateChatSessionRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var session = await _chatService.CreateSessionAsync(userId, request.Title ?? "New Conversation");
                return Ok(session);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to create chat session", error = ex.Message });
            }
        }

        [HttpGet("sessions/{sessionId}/messages")]
        public async Task<IActionResult> GetMessages(Guid sessionId)
        {
            try
            {
                var messages = await _chatService.GetSessionMessagesAsync(sessionId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve messages", error = ex.Message });
            }
        }

        [HttpPost("sessions/{sessionId}/messages")]
        public async Task<IActionResult> SendMessage(Guid sessionId, [FromBody] SendMessageRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _chatService.SendMessageAsync(userId, sessionId, request.Message);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to send message", error = ex.Message });
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                throw new UnauthorizedAccessException("User not authenticated");

            return Guid.Parse(userIdClaim.Value);
        }
    }
}
