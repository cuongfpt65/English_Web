using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using EnglishLearningApp.Data;
using System.Security.Claims;

namespace EnglishLearningApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProgressController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProgressController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProgress()
        {
            try
            {
                var userId = GetCurrentUserId();

                // Get vocabulary statistics
                var totalWords = await _context.Vocabularies.CountAsync();
                var learnedWords = await _context.UserVocabularies
                    .Where(uv => uv.UserId == userId && uv.IsLearned)
                    .CountAsync();

                // Get chat statistics
                var chatSessions = await _context.ChatSessions
                    .Where(cs => cs.UserId == userId)
                    .CountAsync();                var totalMessages = await _context.ChatMessages
                    .Where(cm => cm.ChatSession.UserId == userId)
                    .CountAsync();

                // Calculate streak days
                var streakDays = await CalculateStreakDays(userId);

                // Get recent activity
                var recentActivity = await GetRecentActivity(userId);

                return Ok(new
                {
                    totalWords,
                    learnedWords,
                    chatSessions,
                    totalMessages,
                    streakDays,
                    recentActivity
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve progress", error = ex.Message });
            }
        }

        private async Task<int> CalculateStreakDays(Guid userId)
        {
            try
            {
                // Get all activity dates (vocabulary learned + chat sessions)
                var vocabularyDates = await _context.UserVocabularies
                    .Where(uv => uv.UserId == userId)
                    .Select(uv => uv.AddedAt.Date)
                    .ToListAsync();

                var chatDates = await _context.ChatSessions
                    .Where(cs => cs.UserId == userId)
                    .Select(cs => cs.CreatedAt.Date)
                    .ToListAsync();

                var allDates = vocabularyDates
                    .Concat(chatDates)
                    .Distinct()
                    .OrderByDescending(d => d)
                    .ToList();

                if (!allDates.Any())
                    return 0;

                // Calculate consecutive days streak
                int streak = 1;
                var today = DateTime.UtcNow.Date;
                
                // Check if user was active today or yesterday
                if (allDates.First() < today.AddDays(-1))
                    return 0; // Streak is broken

                for (int i = 0; i < allDates.Count - 1; i++)
                {
                    var diff = (allDates[i] - allDates[i + 1]).Days;
                    if (diff == 1)
                        streak++;
                    else
                        break;
                }

                return streak;
            }
            catch
            {
                return 0;
            }
        }

        private async Task<List<object>> GetRecentActivity(Guid userId)
        {
            try
            {
                var activities = new List<object>();

                // Get recent vocabulary learning
                var recentVocab = await _context.UserVocabularies
                    .Where(uv => uv.UserId == userId)
                    .Include(uv => uv.Vocabulary)
                    .OrderByDescending(uv => uv.AddedAt)
                    .Take(10)
                    .Select(uv => new
                    {
                        Date = uv.AddedAt,
                        Type = "vocabulary",
                        Description = $"Learned word: {uv.Vocabulary.Word}",
                        Topic = uv.Vocabulary.Topic
                    })
                    .ToListAsync();

                // Get recent chat sessions
                var recentChats = await _context.ChatSessions
                    .Where(cs => cs.UserId == userId)
                    .OrderByDescending(cs => cs.CreatedAt)
                    .Take(10)
                    .Select(cs => new
                    {
                        Date = cs.CreatedAt,
                        Type = "chat",
                        Description = cs.Title ?? "Chat session",
                        Topic = (string?)null
                    })
                    .ToListAsync();

                // Combine and sort all activities
                activities.AddRange(recentVocab.Cast<object>());
                activities.AddRange(recentChats.Cast<object>());

                return activities
                    .OrderByDescending(a => 
                    {
                        var dateProperty = a.GetType().GetProperty("Date");
                        return dateProperty?.GetValue(a) as DateTime?;
                    })
                    .Take(10)
                    .ToList();
            }
            catch
            {
                return new List<object>();
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
                return userId;
            
            throw new UnauthorizedAccessException("User not authenticated");
        }
    }
}
