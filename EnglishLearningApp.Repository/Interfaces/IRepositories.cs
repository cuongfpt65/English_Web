using EnglishLearningApp.Data.Entities.User;
using EnglishLearningApp.Data.Entities.Chatbot;
using EnglishLearningApp.Data.Entities.Class;
using EnglishLearningApp.Data.Entities.Test;

namespace EnglishLearningApp.Repository.Interfaces;



public interface IVocabularyRepository
{
    Task<(IEnumerable<Vocabulary> Items, int TotalCount)> GetPaginatedAsync(
        int page, int pageSize, string? search = null, string? topic = null, string? level = null);
    Task<Vocabulary?> GetByIdAsync(Guid id);
    Task<IEnumerable<Vocabulary>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<Vocabulary> CreateAsync(Vocabulary vocabulary);
    Task<Vocabulary> UpdateAsync(Vocabulary vocabulary);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<string>> GetTopicsAsync();
    Task<IEnumerable<string>> GetLevelsAsync();
}

public interface IUserVocabularyRepository
{
    Task<IEnumerable<UserVocabulary>> GetUserVocabulariesAsync(Guid userId, bool? isLearned = null);
    Task<UserVocabulary?> GetUserVocabularyAsync(Guid userId, Guid vocabularyId);
    Task<UserVocabulary> CreateOrUpdateAsync(UserVocabulary userVocabulary);
    Task<bool> ToggleLearnedAsync(Guid userId, Guid vocabularyId);
    Task<bool> AddNoteAsync(Guid userId, Guid vocabularyId, string note);
}

public interface IClassRepository
{
    Task<IEnumerable<ClassRoom>> GetUserClassesAsync(Guid userId);
    Task<ClassRoom?> GetByIdAsync(Guid id, bool includeMembers = false);
    Task<ClassRoom?> GetByCodeAsync(string code);
    Task<ClassRoom> CreateAsync(ClassRoom classEntity);
    Task<ClassRoom> UpdateAsync(ClassRoom classEntity);
    Task<bool> DeleteAsync(Guid id);
}

public interface IClassMemberRepository
{
    Task<IEnumerable<ClassMember>> GetClassMembersAsync(Guid classId);
    Task<ClassMember?> GetMemberAsync(Guid classId, Guid userId);
    Task<ClassMember> AddMemberAsync(ClassMember member);
    Task<bool> RemoveMemberAsync(Guid classId, Guid userId);
    Task<bool> IsMemberAsync(Guid classId, Guid userId);
}


