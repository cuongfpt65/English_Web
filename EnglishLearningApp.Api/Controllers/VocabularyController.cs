using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using EnglishLearningApp.Data;
using EnglishLearningApp.Data.Entities.Chatbot;
using System.Security.Claims;

namespace EnglishLearningApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class VocabularyController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VocabularyController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetVocabulary(
            [FromQuery] string? topic = null,
            [FromQuery] string? level = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = _context.Vocabularies.AsQueryable();

                if (!string.IsNullOrEmpty(topic))
                    query = query.Where(v => v.Topic == topic);

                if (!string.IsNullOrEmpty(level))
                    query = query.Where(v => v.Level == level);

                var totalItems = await query.CountAsync();
                var vocabulary = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(v => new
                    {
                        v.Id,
                        v.Word,
                        v.Meaning,
                        v.Example,
                        v.Topic,
                        v.Level,
                        v.ImageUrl
                    })
                    .ToListAsync();

                return Ok(new
                {
                    Items = vocabulary,
                    TotalItems = totalItems,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve vocabulary", error = ex.Message });
            }
        }

        [HttpGet("topics")]
        public async Task<IActionResult> GetTopics()
        {
            try
            {
                var topics = await _context.Vocabularies
                    .Where(v => !string.IsNullOrEmpty(v.Topic))
                    .Select(v => v.Topic)
                    .Distinct()
                    .ToListAsync();

                return Ok(topics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve topics", error = ex.Message });
            }
        }

        [HttpGet("levels")]
        public async Task<IActionResult> GetLevels()
        {
            try
            {
                var levels = await _context.Vocabularies
                    .Where(v => !string.IsNullOrEmpty(v.Level))
                    .Select(v => v.Level)
                    .Distinct()
                    .ToListAsync();

                return Ok(levels);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve levels", error = ex.Message });
            }
        }

        [HttpPost("{vocabularyId}/learn")]
        public async Task<IActionResult> MarkAsLearned(Guid vocabularyId, [FromBody] LearnVocabularyRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                var vocabulary = await _context.Vocabularies.FindAsync(vocabularyId);
                if (vocabulary == null)
                {
                    return NotFound(new { message = "Vocabulary not found" });
                }

                var userVocabulary = await _context.UserVocabularies
                    .FirstOrDefaultAsync(uv => uv.UserId == userId && uv.VocabularyId == vocabularyId);

                if (userVocabulary == null)
                {
                    userVocabulary = new UserVocabulary
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        VocabularyId = vocabularyId,
                        IsLearned = true,
                        Note = request.Note,
                        AddedAt = DateTime.UtcNow
                    };
                    _context.UserVocabularies.Add(userVocabulary);
                }
                else
                {
                    userVocabulary.IsLearned = true;
                    userVocabulary.Note = request.Note;
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Vocabulary marked as learned" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to mark vocabulary as learned", error = ex.Message });
            }
        }

        [HttpGet("my-vocabulary")]
        public async Task<IActionResult> GetMyVocabulary([FromQuery] bool learnedOnly = false)
        {
            try
            {
                var userId = GetCurrentUserId();
                  var query = _context.UserVocabularies
                    .Where(uv => uv.UserId == userId);

                if (learnedOnly)
                    query = query.Where(uv => uv.IsLearned);

                var userVocabulary = await query
                    .Include(uv => uv.Vocabulary)
                    .OrderByDescending(uv => uv.AddedAt)
                    .Select(uv => new
                    {
                        uv.Id,
                        uv.IsLearned,
                        uv.Note,
                        uv.AddedAt,
                        Vocabulary = new
                        {
                            uv.Vocabulary.Id,
                            uv.Vocabulary.Word,
                            uv.Vocabulary.Meaning,
                            uv.Vocabulary.Example,
                            uv.Vocabulary.Topic,
                            uv.Vocabulary.Level
                        }
                    })
                    .ToListAsync();

                return Ok(userVocabulary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve user vocabulary", error = ex.Message });
            }
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetUserVocabulary()
        {
            try
            {
                var userId = GetCurrentUserId();
                
                var userVocabularies = await _context.UserVocabularies
                    .Where(uv => uv.UserId == userId)
                    .Select(uv => new
                    {
                        uv.Id,
                        VocabularyId = uv.VocabularyId,
                        uv.IsLearned,
                        uv.Note
                    })
                    .ToListAsync();

                return Ok(userVocabularies);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get user vocabulary", error = ex.Message });
            }
        }

        [HttpPost("{vocabularyId}/toggle-learned")]
        public async Task<IActionResult> ToggleLearnedStatus(Guid vocabularyId)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                var userVocabulary = await _context.UserVocabularies
                    .FirstOrDefaultAsync(uv => uv.UserId == userId && uv.VocabularyId == vocabularyId);

                if (userVocabulary == null)
                {
                    // Create new entry
                    userVocabulary = new UserVocabulary
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        VocabularyId = vocabularyId,
                        IsLearned = true,
                        AddedAt = DateTime.UtcNow
                    };
                    _context.UserVocabularies.Add(userVocabulary);
                }
                else
                {
                    // Toggle status
                    userVocabulary.IsLearned = !userVocabulary.IsLearned;
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Status toggled successfully", isLearned = userVocabulary.IsLearned });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to toggle learned status", error = ex.Message });
            }
        }

        [HttpPost("{vocabularyId}/note")]
        public async Task<IActionResult> AddNote(Guid vocabularyId, [FromBody] AddNoteRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                var userVocabulary = await _context.UserVocabularies
                    .FirstOrDefaultAsync(uv => uv.UserId == userId && uv.VocabularyId == vocabularyId);

                if (userVocabulary == null)
                {
                    // Create new entry with note
                    userVocabulary = new UserVocabulary
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        VocabularyId = vocabularyId,
                        Note = request.Note,
                        AddedAt = DateTime.UtcNow
                    };
                    _context.UserVocabularies.Add(userVocabulary);
                }
                else
                {
                    userVocabulary.Note = request.Note;
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Note added successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to add note", error = ex.Message });
            }
        }

        [HttpPost("seed")]
        [AllowAnonymous] // Temporary for seeding data
        public async Task<IActionResult> SeedVocabulary()
        {
            try
            {
                if (await _context.Vocabularies.AnyAsync())
                {
                    return Ok(new { message = "Vocabulary already exists" });
                }

                var vocabularies = new[]
                {
                    new Vocabulary { Id = Guid.NewGuid(), Word = "Hello", Meaning = "Xin chào", Example = "Hello, how are you?", Topic = "Greetings", Level = "Beginner" },
                    new Vocabulary { Id = Guid.NewGuid(), Word = "Goodbye", Meaning = "Tạm biệt", Example = "Goodbye, see you later!", Topic = "Greetings", Level = "Beginner" },
                    new Vocabulary { Word = "Beautiful", Meaning = "Đẹp", Example = "She has a beautiful smile.", Topic = "Adjectives", Level = "Beginner" },
                    new Vocabulary { Word = "Intelligent", Meaning = "Thông minh", Example = "He is very intelligent.", Topic = "Adjectives", Level = "Intermediate" },
                    new Vocabulary { Word = "Adventure", Meaning = "Phiêu lưu", Example = "Life is an adventure.", Topic = "Nouns", Level = "Intermediate" },
                    new Vocabulary { Word = "Magnificent", Meaning = "Tuyệt vời", Example = "The view is magnificent.", Topic = "Adjectives", Level = "Advanced" },
                    new Vocabulary { Word = "Perseverance", Meaning = "Sự kiên trì", Example = "Success requires perseverance.", Topic = "Nouns", Level = "Advanced" },
                    new Vocabulary { Word = "Serendipity", Meaning = "Sự may mắn bất ngờ", Example = "Meeting you was pure serendipity.", Topic = "Nouns", Level = "Advanced" }
                };

                foreach (var vocab in vocabularies)
                {
                    if (vocab.Id == Guid.Empty)
                        vocab.Id = Guid.NewGuid();
                }

                _context.Vocabularies.AddRange(vocabularies);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Vocabulary seeded successfully", count = vocabularies.Length });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to seed vocabulary", error = ex.Message });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchVocabulary([FromQuery] string term)
        {
            try
            {
                if (string.IsNullOrEmpty(term))
                {
                    return BadRequest(new { message = "Search term is required" });
                }

                var vocabularies = await _context.Vocabularies
                    .Where(v => v.Word.Contains(term) || 
                               v.Meaning.Contains(term) || 
                               v.Example.Contains(term))
                    .Select(v => new
                    {
                        v.Id,
                        v.Word,
                        v.Meaning,
                        v.Example,
                        v.Topic,
                        v.Level,
                        v.ImageUrl
                    })
                    .ToListAsync();

                return Ok(vocabularies);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to search vocabulary", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddVocabulary([FromBody] CreateVocabularyRequest request)
        {
            try
            {
                var vocabulary = new Vocabulary
                {
                    Id = Guid.NewGuid(),
                    Word = request.Word,
                    Meaning = request.Meaning,
                    Example = request.Example,
                    Topic = request.Topic,
                    Level = request.Level,
                    ImageUrl = request.ImageUrl
                };

                _context.Vocabularies.Add(vocabulary);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Vocabulary added successfully", id = vocabulary.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to add vocabulary", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVocabulary(Guid id, [FromBody] UpdateVocabularyRequest request)
        {
            try
            {
                var vocabulary = await _context.Vocabularies.FindAsync(id);
                if (vocabulary == null)
                {
                    return NotFound(new { message = "Vocabulary not found" });
                }

                vocabulary.Word = request.Word ?? vocabulary.Word;
                vocabulary.Meaning = request.Meaning ?? vocabulary.Meaning;
                vocabulary.Example = request.Example ?? vocabulary.Example;
                vocabulary.Topic = request.Topic ?? vocabulary.Topic;
                vocabulary.Level = request.Level ?? vocabulary.Level;
                vocabulary.ImageUrl = request.ImageUrl ?? vocabulary.ImageUrl;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Vocabulary updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update vocabulary", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVocabulary(Guid id)
        {
            try
            {
                var vocabulary = await _context.Vocabularies.FindAsync(id);
                if (vocabulary == null)
                {
                    return NotFound(new { message = "Vocabulary not found" });
                }

                _context.Vocabularies.Remove(vocabulary);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Vocabulary deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to delete vocabulary", error = ex.Message });
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                throw new UnauthorizedAccessException("User not authenticated");

            return Guid.Parse(userIdClaim.Value);
        }
    }

    public class LearnVocabularyRequest
    {
        public string? Note { get; set; }
    }

    public class AddNoteRequest
    {
        public string Note { get; set; } = "";
    }

    public class CreateVocabularyRequest
    {
        public string Word { get; set; } = "";
        public string Meaning { get; set; } = "";
        public string Example { get; set; } = "";
        public string Topic { get; set; } = "";
        public string Level { get; set; } = "";
        public string? ImageUrl { get; set; }
    }

    public class UpdateVocabularyRequest
    {
        public string? Word { get; set; }
        public string? Meaning { get; set; }
        public string? Example { get; set; }
        public string? Topic { get; set; }
        public string? Level { get; set; }
        public string? ImageUrl { get; set; }
    }

   
}
