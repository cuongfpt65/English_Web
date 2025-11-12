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
    public class ClassQuizController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClassQuizController(AppDbContext context)
        {
            _context = context;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        }

        // GET: api/ClassQuiz/{classId}
        [HttpGet("{classId}")]
        public async Task<IActionResult> GetClassQuizzes(Guid classId)
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

                var quizzes = await _context.ClassQuizzes
                    .Include(q => q.CreatedBy)
                    .Include(q => q.Questions)
                    .Include(q => q.Attempts)
                    .Where(q => q.ClassRoomId == classId && q.IsActive)
                    .Select(q => new
                    {
                        q.Id,
                        q.Title,
                        q.Description,
                        q.CreatedAt,
                        q.DueDate,
                        q.TimeLimit,
                        CreatedBy = new
                        {
                            q.CreatedBy.Id,
                            q.CreatedBy.FullName
                        },
                        QuestionCount = q.Questions.Count,
                        AttemptCount = q.Attempts.Count,
                        MyAttempt = q.Attempts.FirstOrDefault(a => a.UserId == userId),
                        MyBestScore = q.Attempts.Where(a => a.UserId == userId && a.IsCompleted)
                                               .Max(a => (int?)a.Score) ?? 0
                    })
                    .OrderByDescending(q => q.CreatedAt)
                    .ToListAsync();

                return Ok(quizzes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get quizzes", error = ex.Message });
            }
        }

        // POST: api/ClassQuiz/{classId}
        [HttpPost("{classId}")]
        public async Task<IActionResult> CreateQuiz(Guid classId, [FromBody] CreateQuizRequest request)
        {
            try
            {
                var userId = new Guid(GetCurrentUserId());
                
                // Check if user is teacher of this class
                var membership = await _context.ClassMembers
                    .FirstOrDefaultAsync(cm => cm.ClassRoomId == classId && cm.UserId == userId && cm.Role == "Teacher");

                if (membership == null)
                {
                    return Forbid("Only teachers can create quizzes");
                }

                var quiz = new ClassQuiz
                {
                    Id = Guid.NewGuid(),
                    ClassRoomId = classId,
                    CreatedById = userId,
                    Title = request.Title,
                    Description = request.Description,
                    TimeLimit = request.TimeLimit,
                    DueDate = request.DueDate,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.ClassQuizzes.Add(quiz);

                // Add questions
                foreach (var questionRequest in request.Questions)
                {
                    var question = new ClassQuizQuestion
                    {
                        Id = Guid.NewGuid(),
                        QuizId = quiz.Id,
                        VocabularyId = questionRequest.VocabularyId,
                        QuestionType = questionRequest.QuestionType,
                        QuestionText = questionRequest.QuestionText,
                        CorrectAnswer = questionRequest.CorrectAnswer,
                        Options = questionRequest.Options,
                        Order = questionRequest.Order
                    };

                    _context.ClassQuizQuestions.Add(question);
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Quiz created successfully", quizId = quiz.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to create quiz", error = ex.Message });
            }
        }

        // GET: api/ClassQuiz/{quizId}/start
        [HttpGet("{quizId}/start")]
        public async Task<IActionResult> StartQuiz(Guid quizId)
        {
            try
            {
                var userId = new Guid(GetCurrentUserId());
                
                var quiz = await _context.ClassQuizzes
                    .Include(q => q.Questions)
                    .ThenInclude(qq => qq.Vocabulary)
                    .FirstOrDefaultAsync(q => q.Id == quizId && q.IsActive);

                if (quiz == null)
                {
                    return NotFound("Quiz not found");
                }

                // Check if user is member of this class
                var membership = await _context.ClassMembers
                    .FirstOrDefaultAsync(cm => cm.ClassRoomId == quiz.ClassRoomId && cm.UserId == userId);

                if (membership == null)
                {
                    return Forbid("You are not a member of this class");
                }

                // Check if quiz is still available
                if (quiz.DueDate.HasValue && quiz.DueDate.Value < DateTime.UtcNow)
                {
                    return BadRequest("Quiz deadline has passed");
                }

                // Create new attempt
                var attempt = new ClassQuizAttempt
                {
                    Id = Guid.NewGuid(),
                    QuizId = quizId,
                    UserId = userId,
                    Score = 0,
                    TotalQuestions = quiz.Questions.Count,
                    StartedAt = DateTime.UtcNow,
                    IsCompleted = false
                };

                _context.ClassQuizAttempts.Add(attempt);
                await _context.SaveChangesAsync();

                var questions = quiz.Questions.OrderBy(q => q.Order).Select(q => new
                {
                    q.Id,
                    q.QuestionType,
                    q.QuestionText,
                    q.Options,
                    q.Order,
                    Vocabulary = new
                    {
                        q.Vocabulary.Id,
                        q.Vocabulary.Word,
                        q.Vocabulary.Meaning,
                        q.Vocabulary.Example
                    }
                });

                return Ok(new
                {
                    AttemptId = attempt.Id,
                    Quiz = new
                    {
                        quiz.Id,
                        quiz.Title,
                        quiz.Description,
                        quiz.TimeLimit,
                        quiz.DueDate
                    },
                    Questions = questions
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to start quiz", error = ex.Message });
            }
        }

        // POST: api/ClassQuiz/attempts/{attemptId}/submit
        [HttpPost("attempts/{attemptId}/submit")]
        public async Task<IActionResult> SubmitQuiz(Guid attemptId, [FromBody] SubmitQuizRequest request)
        {
            try
            {
                var userId = new Guid(GetCurrentUserId());
                
                var attempt = await _context.ClassQuizAttempts
                    .Include(a => a.Quiz)
                    .ThenInclude(q => q.Questions)
                    .FirstOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId);

                if (attempt == null)
                {
                    return NotFound("Quiz attempt not found");
                }

                if (attempt.IsCompleted)
                {
                    return BadRequest("Quiz already completed");
                }

                var score = 0;
                var totalQuestions = attempt.Quiz.Questions.Count;

                // Process answers
                foreach (var answerRequest in request.Answers)
                {
                    var question = attempt.Quiz.Questions.FirstOrDefault(q => q.Id == answerRequest.QuestionId);
                    if (question != null)
                    {
                        var isCorrect = string.Equals(answerRequest.Answer.Trim(), question.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
                        if (isCorrect) score++;

                        var answer = new ClassQuizAnswer
                        {
                            Id = Guid.NewGuid(),
                            AttemptId = attemptId,
                            QuestionId = answerRequest.QuestionId,
                            UserAnswer = answerRequest.Answer,
                            IsCorrect = isCorrect
                        };

                        _context.ClassQuizAnswers.Add(answer);
                    }
                }

                // Update attempt
                attempt.Score = score;
                attempt.CompletedAt = DateTime.UtcNow;
                attempt.IsCompleted = true;

                // Update user stats
                await UpdateUserStats(userId, attempt.Quiz.ClassRoomId, score, totalQuestions);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Quiz submitted successfully",
                    score = score,
                    totalQuestions = totalQuestions,
                    percentage = totalQuestions > 0 ? (double)score / totalQuestions * 100 : 0
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to submit quiz", error = ex.Message });
            }
        }

        // GET: api/ClassQuiz/{quizId}/results
        [HttpGet("{quizId}/results")]
        public async Task<IActionResult> GetQuizResults(Guid quizId)
        {
            try
            {
                var userId = new Guid(GetCurrentUserId());
                
                var quiz = await _context.ClassQuizzes
                    .FirstOrDefaultAsync(q => q.Id == quizId);

                if (quiz == null)
                {
                    return NotFound("Quiz not found");
                }

                // Check if user is member of this class
                var membership = await _context.ClassMembers
                    .FirstOrDefaultAsync(cm => cm.ClassRoomId == quiz.ClassRoomId && cm.UserId == userId);

                if (membership == null)
                {
                    return Forbid("You are not a member of this class");
                }

                var results = await _context.ClassQuizAttempts
                    .Include(a => a.User)
                    .Where(a => a.QuizId == quizId && a.IsCompleted)
                    .GroupBy(a => a.UserId)
                    .Select(g => new
                    {
                        User = new
                        {
                            g.First().User.Id,
                            g.First().User.FullName
                        },
                        BestScore = g.Max(a => a.Score),
                        TotalQuestions = g.First().TotalQuestions,
                        AttemptCount = g.Count(),
                        BestAttemptDate = g.Where(a => a.Score == g.Max(x => x.Score))
                                          .Min(a => a.CompletedAt)
                    })
                    .OrderByDescending(r => r.BestScore)
                    .ToListAsync();

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get quiz results", error = ex.Message });
            }
        }

        private async Task UpdateUserStats(Guid userId, Guid classRoomId, int score, int totalQuestions)
        {
            var stats = await _context.ClassMemberStats
                .FirstOrDefaultAsync(s => s.UserId == userId && s.ClassRoomId == classRoomId);

            if (stats != null)
            {
                stats.QuizzesCompleted++;
                var totalScore = (stats.AverageQuizScore * (stats.QuizzesCompleted - 1)) + score;
                stats.AverageQuizScore = totalScore / stats.QuizzesCompleted;
                stats.TotalPoints += score * 10; // 10 points per correct answer
                stats.LastUpdated = DateTime.UtcNow;
            }
        }
    }

    public class CreateQuizRequest
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public int TimeLimit { get; set; }
        public DateTime? DueDate { get; set; }
        public List<CreateQuizQuestionRequest> Questions { get; set; } = new();
    }

    public class CreateQuizQuestionRequest
    {
        public Guid VocabularyId { get; set; }
        public string QuestionType { get; set; } = "";
        public string QuestionText { get; set; } = "";
        public string CorrectAnswer { get; set; } = "";
        public string[]? Options { get; set; }
        public int Order { get; set; }
    }

    public class SubmitQuizRequest
    {
        public List<QuizAnswerRequest> Answers { get; set; } = new();
    }

    public class QuizAnswerRequest
    {
        public Guid QuestionId { get; set; }
        public string Answer { get; set; } = "";
    }
}
