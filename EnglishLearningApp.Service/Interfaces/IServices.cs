namespace EnglishLearningApp.Service.Interfaces;

public interface IAuthService
{
    Task<object> LoginAsync(object request);
    Task<object> RegisterAsync(object request);
    Task<object> LoginWithPhoneAsync(object request);
    Task<string> SendVerificationCodeAsync(object request);
}

public interface IVocabularyService
{
    Task<object> GetPaginatedAsync(int page, int pageSize, string? topic = null, string? level = null);
    Task<object?> GetByIdAsync(Guid id);
    Task<object> CreateAsync(object dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<string>> GetTopicsAsync();
    Task<IEnumerable<string>> GetLevelsAsync();
    
    // User vocabulary methods
    Task<object> GetUserVocabulariesAsync(Guid userId, bool? isLearned = null);
    Task<bool> ToggleLearnedAsync(Guid userId, Guid vocabularyId);
    Task<bool> AddNoteAsync(Guid userId, Guid vocabularyId, string note);
}

public interface IClassService
{
    Task<object> GetUserClassesAsync(Guid userId);
    Task<object?> GetByIdAsync(Guid id);
    Task<object> CreateAsync(Guid userId, string name, string? description);
    Task<object?> JoinClassAsync(Guid userId, string code);
    Task<bool> LeaveClassAsync(Guid userId, Guid classId);
    Task<object> GetMembersAsync(Guid classId);
    
    // Quiz methods
    Task<object> GetClassQuizzesAsync(Guid classId);
    Task<object?> GetQuizAsync(Guid id);
    Task<object> CreateQuizAsync(Guid userId, Guid classId, object dto);
    Task<object> SubmitQuizAsync(Guid userId, Guid quizId, object submission);
    Task<object> GetQuizResultsAsync(Guid quizId);
}

public interface IChatService
{
    Task<object> GetUserSessionsAsync(Guid userId);
    Task<object> CreateSessionAsync(Guid userId, string title);
    Task<object> GetSessionMessagesAsync(Guid sessionId);
    Task<object> SendMessageAsync(Guid userId, Guid sessionId, string message);
}
