using EnglishLearningApp.Data;
using EnglishLearningApp.Data.Entities.User;
using EnglishLearningApp.Data.Entities.Chatbot;
using EnglishLearningApp.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningApp.Repository.Implementations;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppUser?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<AppUser?> GetByPhoneAsync(string phoneNumber)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
    }

    public async Task<AppUser?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<AppUser> CreateAsync(AppUser user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<AppUser> UpdateAsync(AppUser user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> ExistsAsync(string email, string? phoneNumber = null)
    {
        var query = _context.Users.Where(u => u.Email == email);
        
        if (!string.IsNullOrEmpty(phoneNumber))
        {
            query = query.Where(u => u.PhoneNumber == phoneNumber);
        }
        
        return await query.AnyAsync();
    }
}

/*public class VocabularyRepository : IVocabularyRepository
{
    private readonly AppDbContext _context;

    public VocabularyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<Vocabulary> Items, int TotalCount)> GetPaginatedAsync(
        int page, int pageSize, string? search = null, string? topic = null, string? level = null)
    {
        var query = _context.Vocabularies.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(v => v.Word.Contains(search) || v.Meaning.Contains(search));
        }

        if (!string.IsNullOrEmpty(topic))
        {
            query = query.Where(v => v.Topic == topic);
        }

        if (!string.IsNullOrEmpty(level))
        {
            query = query.Where(v => v.Level == level);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(v => v.Word)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Vocabulary?> GetByIdAsync(Guid id)
    {
        return await _context.Vocabularies.FindAsync(id);
    }

    public async Task<IEnumerable<Vocabulary>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        return await _context.Vocabularies.Where(v => ids.Contains(v.Id)).ToListAsync();
    }

    public async Task<Vocabulary> CreateAsync(Vocabulary vocabulary)
    {
        vocabulary.Id = Guid.NewGuid();
        vocabulary.CreatedAt = DateTime.UtcNow;
        
        _context.Vocabularies.Add(vocabulary);
        await _context.SaveChangesAsync();
        return vocabulary;
    }

    public async Task<Vocabulary> UpdateAsync(Vocabulary vocabulary)
    {
        _context.Vocabularies.Update(vocabulary);
        await _context.SaveChangesAsync();
        return vocabulary;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var vocabulary = await _context.Vocabularies.FindAsync(id);
        if (vocabulary == null) return false;

        _context.Vocabularies.Remove(vocabulary);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<string>> GetTopicsAsync()
    {
        return await _context.Vocabularies
            .Where(v => !string.IsNullOrEmpty(v.Topic))
            .Select(v => v.Topic!)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetLevelsAsync()
    {
        return await _context.Vocabularies
            .Where(v => !string.IsNullOrEmpty(v.Level))
            .Select(v => v.Level!)
            .Distinct()
            .OrderBy(l => l)
            .ToListAsync();
    }*/
//}
