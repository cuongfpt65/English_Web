using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using EnglishLearningApp.Data;
using EnglishLearningApp.Data.Entities.MyVocab;
using System.Security.Claims;
using EnglishLearningApp.Data.Entities.Chatbot;

namespace EnglishLearningApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class MyVocabController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MyVocabController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get current user's ID from JWT token
        /// </summary>
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }
            return userId;
        }

        /// <summary>
        /// GET: api/MyVocab - Get all personal vocabulary of current user
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyVocabulary(
            [FromQuery] string? topic = null,
            [FromQuery] string? level = null,
            [FromQuery] bool? isLearned = null,
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = GetCurrentUserId();
                var query = _context.MyVocabs
                    .Where(v => v.UserId == userId)
                    .AsQueryable();

                // Filters
                if (!string.IsNullOrEmpty(topic))
                    query = query.Where(v => v.Topic == topic);

                if (!string.IsNullOrEmpty(level))
                    query = query.Where(v => v.Level == level);

                if (isLearned.HasValue)
                    query = query.Where(v => v.IsLearned == isLearned.Value);

                if (!string.IsNullOrEmpty(search))
                    query = query.Where(v => v.Word.Contains(search) || v.Meaning.Contains(search));

                var totalItems = await query.CountAsync();
                var vocabulary = await query
                    .OrderByDescending(v => v.CreatedAt)
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
                        v.ImageUrl,
                        v.Note,
                        v.IsLearned,
                        v.CreatedAt,
                        v.UpdatedAt
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
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve vocabulary", error = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/MyVocab/{id} - Get a specific vocabulary by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMyVocabularyById(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var vocab = await _context.MyVocabs
                    .Where(v => v.Id == id && v.UserId == userId)
                    .Select(v => new
                    {
                        v.Id,
                        v.Word,
                        v.Meaning,
                        v.Example,
                        v.Topic,
                        v.Level,
                        v.ImageUrl,
                        v.Note,
                        v.IsLearned,
                        v.CreatedAt,
                        v.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (vocab == null)
                {
                    return NotFound(new { message = "Vocabulary not found or access denied" });
                }

                return Ok(vocab);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve vocabulary", error = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/MyVocab - Create new personal vocabulary
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateMyVocabulary([FromBody] CreateMyVocabDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();

                // Check if word already exists for this user
                var exists = await _context.MyVocabs
                    .AnyAsync(v => v.UserId == userId && v.Word.ToLower() == dto.Word.ToLower());

                if (exists)
                {
                    return BadRequest(new { message = "This word already exists in your vocabulary" });
                }

                var vocab = new MyVocab
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Word = dto.Word.Trim(),
                    Meaning = dto.Meaning.Trim(),
                    Example = dto.Example?.Trim(),
                    ImageUrl = dto.ImageUrl?.Trim(),
                    Topic = dto.Topic?.Trim(),
                    Level = dto.Level?.Trim(),
                    Note = dto.Note?.Trim(),
                    IsLearned = dto.IsLearned ?? false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.MyVocabs.Add(vocab);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetMyVocabularyById), new { id = vocab.Id }, new
                {
                    vocab.Id,
                    vocab.Word,
                    vocab.Meaning,
                    vocab.Example,
                    vocab.Topic,
                    vocab.Level,
                    vocab.ImageUrl,
                    vocab.Note,
                    vocab.IsLearned,
                    vocab.CreatedAt,
                    vocab.UpdatedAt
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to create vocabulary", error = ex.Message });
            }
        }

        /// <summary>
        /// PUT: api/MyVocab/{id} - Update personal vocabulary
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMyVocabulary(Guid id, [FromBody] UpdateMyVocabDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var vocab = await _context.MyVocabs
                    .FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId);

                if (vocab == null)
                {
                    return NotFound(new { message = "Vocabulary not found or access denied" });
                }

                // Update fields
                vocab.Word = dto.Word?.Trim() ?? vocab.Word;
                vocab.Meaning = dto.Meaning?.Trim() ?? vocab.Meaning;
                vocab.Example = dto.Example?.Trim();
                vocab.ImageUrl = dto.ImageUrl?.Trim();
                vocab.Topic = dto.Topic?.Trim();
                vocab.Level = dto.Level?.Trim();
                vocab.Note = dto.Note?.Trim();
                vocab.IsLearned = dto.IsLearned ?? vocab.IsLearned;
                vocab.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    vocab.Id,
                    vocab.Word,
                    vocab.Meaning,
                    vocab.Example,
                    vocab.Topic,
                    vocab.Level,
                    vocab.ImageUrl,
                    vocab.Note,
                    vocab.IsLearned,
                    vocab.CreatedAt,
                    vocab.UpdatedAt,
                    message = "Vocabulary updated successfully"
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update vocabulary", error = ex.Message });
            }
        }

        /// <summary>
        /// DELETE: api/MyVocab/{id} - Delete personal vocabulary
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMyVocabulary(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var vocab = await _context.MyVocabs
                    .FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId);

                if (vocab == null)
                {
                    return NotFound(new { message = "Vocabulary not found or access denied" });
                }

                _context.MyVocabs.Remove(vocab);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Vocabulary deleted successfully" });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to delete vocabulary", error = ex.Message });
            }
        }

        /// <summary>
        /// PATCH: api/MyVocab/{id}/toggle-learned - Toggle learned status
        /// </summary>
        [HttpPatch("{id}/toggle-learned")]
        public async Task<IActionResult> ToggleLearned(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var vocab = await _context.MyVocabs
                    .FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId);

                if (vocab == null)
                {
                    return NotFound(new { message = "Vocabulary not found or access denied" });
                }

                vocab.IsLearned = !vocab.IsLearned;
                vocab.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    vocab.Id,
                    vocab.IsLearned,
                    message = $"Vocabulary marked as {(vocab.IsLearned ? "learned" : "not learned")}"
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update vocabulary", error = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/MyVocab/statistics - Get vocabulary statistics for current user
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var userId = GetCurrentUserId();
                var total = await _context.MyVocabs.CountAsync(v => v.UserId == userId);
                var learned = await _context.MyVocabs.CountAsync(v => v.UserId == userId && v.IsLearned);
                var topics = await _context.MyVocabs
                    .Where(v => v.UserId == userId && !string.IsNullOrEmpty(v.Topic))
                    .GroupBy(v => v.Topic)
                    .Select(g => new { Topic = g.Key, Count = g.Count() })
                    .ToListAsync();

                return Ok(new
                {
                    TotalWords = total,
                    LearnedWords = learned,
                    NotLearnedWords = total - learned,
                    TopicBreakdown = topics
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get statistics", error = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/MyVocab/topics - Get all unique topics
        /// </summary>
        [HttpGet("topics")]
        public async Task<IActionResult> GetTopics()
        {
            try
            {
                var userId = GetCurrentUserId();
                var topics = await _context.MyVocabs
                    .Where(v => v.UserId == userId && !string.IsNullOrEmpty(v.Topic))
                    .Select(v => v.Topic)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToListAsync();

                return Ok(topics);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve topics", error = ex.Message });
            }
        }        /// <summary>
        /// GET: api/MyVocab/levels - Get all unique levels
        /// </summary>
        [HttpGet("levels")]
        public async Task<IActionResult> GetLevels()
        {
            try
            {
                var userId = GetCurrentUserId();
                var levels = await _context.MyVocabs
                    .Where(v => v.UserId == userId && !string.IsNullOrEmpty(v.Level))
                    .Select(v => v.Level)
                    .Distinct()
                    .OrderBy(l => l)
                    .ToListAsync();

                return Ok(levels);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve levels", error = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/MyVocab/{id}/add-to-vocabulary - Add a word from MyVocab to global Vocabulary
        /// </summary>
        [HttpPost("{id}/add-to-vocabulary")]
        public async Task<IActionResult> AddToGlobalVocabulary(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                // Get the MyVocab word
                var myVocab = await _context.MyVocabs
                    .FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId);

                if (myVocab == null)
                {
                    return NotFound(new { message = "Word not found in your vocabulary" });
                }

                // Check if word already exists in global vocabulary
                var existingVocab = await _context.Vocabularies
                    .FirstOrDefaultAsync(v => v.Word.ToLower() == myVocab.Word.ToLower());

                if (existingVocab != null)
                {
                    return Ok(new
                    {
                        success = false,
                        message = $"Word '{myVocab.Word}' already exists in global vocabulary",
                        vocabularyId = existingVocab.Id
                    });
                }

                // Create new global vocabulary entry
                var newVocab = new Vocabulary
                {
                    Id = Guid.NewGuid(),
                    Word = myVocab.Word,
                    Meaning = myVocab.Meaning,
                    Example = myVocab.Example,
                    Topic = myVocab.Topic ?? "General",
                    Level = myVocab.Level ?? "Intermediate",
                    ImageUrl = myVocab.ImageUrl,
                    
                };
                

                _context.Vocabularies.Add(newVocab);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Word '{myVocab.Word}' has been added to global vocabulary",
                    vocabularyId = newVocab.Id
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to add word to global vocabulary", error = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/MyVocab/add-to-vocabulary-batch - Add multiple words from MyVocab to global Vocabulary
        /// </summary>
        [HttpPost("add-to-vocabulary-batch")]
        public async Task<IActionResult> AddMultipleToGlobalVocabulary([FromBody] AddToVocabularyBatchDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();

                var result = new
                {
                    addedCount = 0,
                    skippedCount = 0,
                    failedCount = 0,
                    addedWords = new List<object>(),
                    skippedWords = new List<object>(),
                    failedWords = new List<object>()
                };

                var addedWords = new List<object>();
                var skippedWords = new List<object>();
                var failedWords = new List<object>();

                foreach (var myVocabId in dto.Ids)
                {
                    try
                    {
                        // Get the MyVocab word
                        var myVocab = await _context.MyVocabs
                            .FirstOrDefaultAsync(v => v.Id == myVocabId && v.UserId == userId);

                        if (myVocab == null)
                        {
                            failedWords.Add(new
                            {
                                myVocabId,
                                word = "Unknown",
                                error = "Word not found in your vocabulary"
                            });
                            continue;
                        }

                        // Check if word already exists in global vocabulary
                        var existingVocab = await _context.Vocabularies
                            .FirstOrDefaultAsync(v => v.Word.ToLower() == myVocab.Word.ToLower());

                        if (existingVocab != null)
                        {
                            skippedWords.Add(new
                            {
                                myVocabId,
                                word = myVocab.Word,
                                reason = "Already exists in global vocabulary"
                            });
                            continue;
                        }

                        // Create new global vocabulary entry
                        var newVocab = new Vocabulary
                        {
                            Id = Guid.NewGuid(),
                            Word = myVocab.Word,
                            Meaning = myVocab.Meaning,
                            Example = myVocab.Example,
                            Topic = myVocab.Topic ?? "General",
                            Level = myVocab.Level ?? "Intermediate",
                            ImageUrl = myVocab.ImageUrl,
                           
                        };

                        _context.Vocabularies.Add(newVocab);
                        await _context.SaveChangesAsync();

                        addedWords.Add(new
                        {
                            myVocabId,
                            vocabularyId = newVocab.Id,
                            word = myVocab.Word
                        });
                    }
                    catch (Exception ex)
                    {
                        failedWords.Add(new
                        {
                            myVocabId,
                            word = "Unknown",
                            error = ex.Message
                        });
                    }
                }

                return Ok(new
                {
                    addedCount = addedWords.Count,
                    skippedCount = skippedWords.Count,
                    failedCount = failedWords.Count,
                    addedWords,
                    skippedWords,
                    failedWords
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to add words to global vocabulary", error = ex.Message });
            }
        }
    }

    // DTOs
    public class CreateMyVocabDto
    {
        public string Word { get; set; } = "";
        public string Meaning { get; set; } = "";
        public string? Example { get; set; }
        public string? ImageUrl { get; set; }
        public string? Topic { get; set; }
        public string? Level { get; set; }
        public string? Note { get; set; }
        public bool? IsLearned { get; set; }
    }    public class UpdateMyVocabDto
    {
        public string? Word { get; set; }
        public string? Meaning { get; set; }
        public string? Example { get; set; }
        public string? ImageUrl { get; set; }
        public string? Topic { get; set; }
        public string? Level { get; set; }
        public string? Note { get; set; }
        public bool? IsLearned { get; set; }
    }

    public class AddToVocabularyBatchDto
    {
        public List<Guid> Ids { get; set; } = new();
    }
}
