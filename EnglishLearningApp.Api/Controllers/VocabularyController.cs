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
    [Authorize] // Yêu cầu authentication cho tất cả endpoints
    public class VocabularyController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VocabularyController(AppDbContext context)
        {
            _context = context;
        }        [HttpGet]
        [AllowAnonymous] // Cho phép truy cập không cần đăng nhập
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
        }        [HttpGet("topics")]
        [AllowAnonymous] // Cho phép truy cập không cần đăng nhập
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
        }        [HttpGet("levels")]
        [AllowAnonymous] // Cho phép truy cập không cần đăng nhập
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
        }        // -------------------------------------------------------------
        // 🤖 AI Quiz Generation - Tạo quiz AI từ vocabulary
        // -------------------------------------------------------------
        [HttpPost("generate-ai-quiz")]
        [AllowAnonymous] // Tạm thời cho phép không cần đăng nhập để test
        public async Task<IActionResult> GenerateAIQuiz([FromBody] GenerateAIQuizRequest request)
        {
            try
            {
                // Lấy userId từ token nếu có, nếu không thì dùng test user
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid userId;
                
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out userId))
                {
                    // Tạm thời dùng user đầu tiên trong database để test
                    var testUser = await _context.Users.FirstOrDefaultAsync();
                    if (testUser == null)
                    {
                        return BadRequest(new { message = "No users found in database. Please create a user first." });
                    }
                    userId = testUser.Id;
                }                // Lấy danh sách vocabulary từ hệ thống (không cần user đã học)
                var allVocabularies = await _context.Vocabularies
                    .OrderBy(v => Guid.NewGuid()) // Random order
                    .ToListAsync();

                if (allVocabularies.Count == 0)
                {
                    return BadRequest(new { message = "Hệ thống chưa có từ vựng nào. Vui lòng thêm từ vựng trước!" });
                }

                // Giới hạn số lượng từ theo yêu cầu
                var count = Math.Min(request.Count, allVocabularies.Count);
                
                // Lấy ngẫu nhiên số lượng từ
                var selectedVocabularies = allVocabularies
                    .Take(count)
                    .Select(v => new
                    {
                        v.Word,
                        v.Meaning,
                        v.Example
                    })
                    .ToList();

                // Chuyển thành JSON để gửi cho AI
                var vocabularyJson = System.Text.Json.JsonSerializer.Serialize(selectedVocabularies);

                // Gọi ChatBotService để tạo quiz
                var chatBotService = HttpContext.RequestServices.GetRequiredService<ERSP.Api.Services.ChatBotService>();
                var quizJson = await chatBotService.HandleAsync(vocabularyJson, "ai_quiz");

                return Ok(new
                {
                    quiz = System.Text.Json.JsonSerializer.Deserialize<object>(quizJson),
                    vocabularyCount = count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to generate AI quiz", error = ex.Message });
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
        }        [HttpGet("search")]
        [AllowAnonymous] // Cho phép truy cập không cần đăng nhập
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
        }        // Check if words already exist in global Vocabulary table
        [HttpPost("check-exists")]
        public async Task<IActionResult> CheckVocabularyExists([FromBody] CheckVocabularyRequest request)
        {
            try
            {
                // Check if words exist in the global Vocabularies table
                var existingWords = await _context.Vocabularies
                    .Where(v => request.Words.Select(w => w.ToLower()).Contains(v.Word.ToLower()))
                    .Select(v => v.Word)
                    .ToListAsync();

                return Ok(new { existingWords });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to check vocabulary", error = ex.Message });
            }
        }

        // Add multiple vocabularies at once (batch)
        [HttpPost("batch")]
        public async Task<IActionResult> AddVocabularyBatch([FromBody] BatchVocabularyRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                int addedCount = 0;
                int skippedCount = 0;

                foreach (var wordData in request.Words)
                {
                    // Check if vocabulary already exists in system
                    var existingVocab = await _context.Vocabularies
                        .FirstOrDefaultAsync(v => v.Word.ToLower() == wordData.Word.ToLower());

                    Vocabulary vocab;
                    if (existingVocab == null)
                    {
                        // Create new vocabulary
                        vocab = new Vocabulary
                        {
                            Id = Guid.NewGuid(),
                            Word = wordData.Word,
                            Meaning = wordData.Meaning,
                            Example = wordData.Example ?? "",
                            Topic = wordData.Topic ?? "General",
                            Level = wordData.Level ?? "Intermediate",
                            ImageUrl = null
                        };
                        _context.Vocabularies.Add(vocab);
                    }
                    else
                    {
                        vocab = existingVocab;
                    }

                    // Check if user already learned this word
                    var userVocabExists = await _context.UserVocabularies
                        .AnyAsync(uv => uv.UserId == userId && uv.VocabularyId == vocab.Id);

                    if (!userVocabExists)
                    {
                        // Add to user's vocabulary
                        var userVocab = new UserVocabulary
                        {
                            Id = Guid.NewGuid(),
                            UserId = userId,
                            VocabularyId = vocab.Id,
                            Note = "From AI Chat",
                            AddedAt = DateTime.UtcNow
                        };
                        _context.UserVocabularies.Add(userVocab);
                        addedCount++;
                    }
                    else
                    {
                        skippedCount++;
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = $"Added {addedCount} new words, skipped {skippedCount} existing words",
                    addedCount,
                    skippedCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to add vocabulary batch", error = ex.Message });
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

    public class CheckVocabularyRequest
    {
        public List<string> Words { get; set; } = new();
    }

    public class BatchVocabularyRequest
    {
        public List<WordData> Words { get; set; } = new();
    }

    public class WordData
    {
        public string Word { get; set; } = "";
        public string Meaning { get; set; } = "";
        public string? Example { get; set; }
        public string? Category { get; set; }
        public string? Topic { get; set; }
        public string? Level { get; set; }
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

    public class GenerateAIQuizRequest
    {
        public int Count { get; set; } = 10; // Số lượng từ muốn dùng để tạo quiz
    }
}
