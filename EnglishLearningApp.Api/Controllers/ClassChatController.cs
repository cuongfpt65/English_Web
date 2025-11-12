using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using EnglishLearningApp.Data;
using EnglishLearningApp.Data.Entities.Class;
using System.Security.Claims;

namespace EnglishLearningApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClassChatController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClassChatController(AppDbContext context)
        {
            _context = context;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        }

        // GET: api/ClassChat/{classId}/messages
        [HttpGet("{classId}/messages")]
        public async Task<IActionResult> GetMessages(Guid classId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                var userId = new Guid(GetCurrentUserId());
                
                // Check if user is member of this class
                var membership = await _context.ClassMembers
                    .FirstOrDefaultAsync(cm => cm.ClassRoomId == classId && cm.UserId == userId);

                if (membership == null)
                {
                    return Forbid("You are not a member of this class");
                }

                var messages = await _context.ClassMessages
                    .Include(cm => cm.Sender)
                    .Where(cm => cm.ClassRoomId == classId)
                    .OrderByDescending(cm => cm.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(cm => new
                    {
                        cm.Id,
                        cm.Message,
                        cm.CreatedAt,
                        Sender = new
                        {
                            cm.Sender.Id,
                            cm.Sender.FullName
                        }
                    })
                    .ToListAsync();

                return Ok(messages.OrderBy(m => m.CreatedAt));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get messages", error = ex.Message });
            }
        }

        // POST: api/ClassChat/{classId}/messages
        [HttpPost("{classId}/messages")]
        public async Task<IActionResult> SendMessage(Guid classId, [FromBody] SendMessageRequest request)
        {
            try
            {
                var userId = new Guid(GetCurrentUserId());
                
                // Check if user is member of this class
                var membership = await _context.ClassMembers
                    .FirstOrDefaultAsync(cm => cm.ClassRoomId == classId && cm.UserId == userId);

                if (membership == null)
                {
                    return Forbid("You are not a member of this class");
                }

                var message = new ClassMessage
                {
                    Id = Guid.NewGuid(),
                    ClassRoomId = classId,
                    SenderId = userId,
                    Message = request.Message,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ClassMessages.Add(message);
                await _context.SaveChangesAsync();

                // Return the created message with sender info
                var createdMessage = await _context.ClassMessages
                    .Include(cm => cm.Sender)
                    .Where(cm => cm.Id == message.Id)
                    .Select(cm => new
                    {
                        cm.Id,
                        cm.Message,
                        cm.CreatedAt,
                        Sender = new
                        {
                            cm.Sender.Id,
                            cm.Sender.FullName
                        }
                    })
                    .FirstOrDefaultAsync();

                return Ok(createdMessage);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to send message", error = ex.Message });
            }
        }

        // DELETE: api/ClassChat/messages/{messageId}
        [HttpDelete("messages/{messageId}")]
        public async Task<IActionResult> DeleteMessage(Guid messageId)
        {
            try
            {
                var userId = new Guid(GetCurrentUserId());
                
                var message = await _context.ClassMessages
                    .Include(cm => cm.ClassRoom)
                    .FirstOrDefaultAsync(cm => cm.Id == messageId);

                if (message == null)
                {
                    return NotFound("Message not found");
                }

                // Check if user is the sender or teacher of the class
                var membership = await _context.ClassMembers
                    .FirstOrDefaultAsync(cm => cm.ClassRoomId == message.ClassRoomId && cm.UserId == userId);

                if (membership == null || (message.SenderId != userId && membership.Role != "Teacher"))
                {
                    return Forbid("You don't have permission to delete this message");
                }

                _context.ClassMessages.Remove(message);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Message deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to delete message", error = ex.Message });
            }
        }
    }

    public class SendMessageRequest
    {
        public string Message { get; set; } = "";
    }
}