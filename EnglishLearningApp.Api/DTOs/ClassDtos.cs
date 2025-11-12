namespace EnglishLearningApp.Api.DTOs;

public class ClassDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Code { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int MemberCount { get; set; }
}

public class CreateClassDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class JoinClassDto
{
    public string Code { get; set; } = string.Empty;
}

public class ClassMemberDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public bool IsCreator { get; set; }
}

public class QuizDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TimeLimit { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool IsActive { get; set; }
    public IEnumerable<QuizQuestionDto> Questions { get; set; } = new List<QuizQuestionDto>();
}

public class CreateQuizDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TimeLimit { get; set; }
    public IEnumerable<string> VocabularyIds { get; set; } = new List<string>();
}

public class QuizQuestionDto
{
    public string Id { get; set; } = string.Empty;
    public string Word { get; set; } = string.Empty;
    public IEnumerable<string> Options { get; set; } = new List<string>();
    public int CorrectAnswerIndex { get; set; }
}

public class QuizSubmissionDto
{
    public IEnumerable<QuizAnswerDto> Answers { get; set; } = new List<QuizAnswerDto>();
}

public class QuizAnswerDto
{
    public string QuestionId { get; set; } = string.Empty;
    public int SelectedAnswer { get; set; }
}

public class QuizResultDto
{
    public string Id { get; set; } = string.Empty;
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public double Percentage { get; set; }
    public TimeSpan TimeTaken { get; set; }
    public DateTime CompletedAt { get; set; }
    public IEnumerable<QuizResultDetailDto> Details { get; set; } = new List<QuizResultDetailDto>();
}

public class QuizResultDetailDto
{
    public string QuestionId { get; set; } = string.Empty;
    public string Word { get; set; } = string.Empty;
    public int SelectedAnswer { get; set; }
    public int CorrectAnswer { get; set; }
    public bool IsCorrect { get; set; }
}

public class CreateClassRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class JoinClassRequestDto
{
    public string InviteCode { get; set; } = string.Empty;
}
