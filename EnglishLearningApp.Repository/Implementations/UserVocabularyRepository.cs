using EnglishLearningApp.Data;
using EnglishLearningApp.Data.Entities.Chatbot;
using EnglishLearningApp.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningApp.Repository.Implementations;

public class UserVocabularyRepository : IUserVocabularyRepository
{
    private readonly AppDbContext _context;

    public UserVocabularyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserVocabulary>> GetUserVocabulariesAsync(Guid userId, bool? isLearned = null)
    {
        var query = _context.UserVocabularies
            .Include(uv => uv.Vocabulary)
            .Where(uv => uv.UserId == userId);

        if (isLearned.HasValue)
        {
            query = query.Where(uv => uv.IsLearned == isLearned.Value);
        }

        return await query
            .OrderByDescending(uv => uv.AddedAt)
            .ToListAsync();
    }

    public async Task<UserVocabulary?> GetUserVocabularyAsync(Guid userId, Guid vocabularyId)
    {
        return await _context.UserVocabularies
            .FirstOrDefaultAsync(uv => uv.UserId == userId && uv.VocabularyId == vocabularyId);
    }

    public async Task<UserVocabulary> CreateOrUpdateAsync(UserVocabulary userVocabulary)
    {
        var existing = await GetUserVocabularyAsync(userVocabulary.UserId, userVocabulary.VocabularyId);
        
        if (existing == null)
        {
            userVocabulary.Id = Guid.NewGuid();
            userVocabulary.AddedAt = DateTime.UtcNow;
            _context.UserVocabularies.Add(userVocabulary);
        }
        else
        {
            existing.IsLearned = userVocabulary.IsLearned;
            existing.Note = userVocabulary.Note;
            _context.UserVocabularies.Update(existing);
        }

        await _context.SaveChangesAsync();
        return existing ?? userVocabulary;
    }

    public async Task<bool> ToggleLearnedAsync(Guid userId, Guid vocabularyId)
    {
        var userVocabulary = await GetUserVocabularyAsync(userId, vocabularyId);

        if (userVocabulary == null)
        {
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
            userVocabulary.IsLearned = !userVocabulary.IsLearned;
            _context.UserVocabularies.Update(userVocabulary);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddNoteAsync(Guid userId, Guid vocabularyId, string note)
    {
        var userVocabulary = await GetUserVocabularyAsync(userId, vocabularyId);

        if (userVocabulary == null)
        {
            userVocabulary = new UserVocabulary
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                VocabularyId = vocabularyId,
                Note = note,
                AddedAt = DateTime.UtcNow
            };
            _context.UserVocabularies.Add(userVocabulary);
        }
        else
        {
            userVocabulary.Note = note;
            _context.UserVocabularies.Update(userVocabulary);
        }

        await _context.SaveChangesAsync();
        return true;
    }
}
