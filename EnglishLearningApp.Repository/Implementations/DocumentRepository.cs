using EnglishLearningApp.Data;
using EnglishLearningApp.Data.Entities.Document;
using EnglishLearningApp.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningApp.Repository.Implementations;

public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _context;

    public DocumentRepository(AppDbContext context)
    {
        _context = context;
    }

    #region Category Methods

    public async Task<IEnumerable<DocumentCategory>> GetAllCategoriesAsync()
    {
        return await _context.DocumentCategories
            .Include(c => c.Documents)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<DocumentCategory?> GetCategoryByIdAsync(Guid id)
    {
        return await _context.DocumentCategories
            .Include(c => c.Documents)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<DocumentCategory> CreateCategoryAsync(DocumentCategory category)
    {
        category.Id = Guid.NewGuid();
        category.CreatedAt = DateTime.UtcNow;
        _context.DocumentCategories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<DocumentCategory> UpdateCategoryAsync(DocumentCategory category)
    {
        _context.DocumentCategories.Update(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<bool> DeleteCategoryAsync(Guid id)
    {
        var category = await _context.DocumentCategories.FindAsync(id);
        if (category == null) return false;        _context.DocumentCategories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Document Methods

    public async Task<(IEnumerable<Document> Items, int TotalCount)> GetDocumentsAsync(
        Guid? categoryId = null,
        string? search = null,
        string? fileType = null,
        int page = 1,
        int pageSize = 10,
        Guid? uploaderId = null)
    {
        var query = _context.Documents
            .Include(d => d.Category)
            .Include(d => d.UploadedBy)
            .AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(d => d.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(d => 
                d.Title.Contains(search) || 
                d.Description.Contains(search) ||
                d.FileName.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(fileType))
        {
            query = query.Where(d => d.FileType.ToLower() == fileType.ToLower());
        }

        if (uploaderId.HasValue)
        {
            query = query.Where(d => d.UploadedByUserId == uploaderId.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Document?> GetDocumentByIdAsync(Guid id)
    {
        return await _context.Documents
            .Include(d => d.Category)
            .Include(d => d.UploadedBy)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<Document> CreateDocumentAsync(Document document)
    {
        document.Id = Guid.NewGuid();
        document.CreatedAt = DateTime.UtcNow;
        document.UpdatedAt = DateTime.UtcNow;
        _context.Documents.Add(document);
        await _context.SaveChangesAsync();
        return document;
    }

    public async Task<Document> UpdateDocumentAsync(Document document)
    {
        document.UpdatedAt = DateTime.UtcNow;
        _context.Documents.Update(document);
        await _context.SaveChangesAsync();
        return document;
    }    public async Task<bool> DeleteDocumentAsync(Guid id)
    {
        var document = await _context.Documents.FindAsync(id);
        if (document == null) return false;

        // Delete all related UserDocumentHistories first to avoid FK constraint error
        var histories = _context.UserDocumentHistories.Where(h => h.DocumentId == id);
        _context.UserDocumentHistories.RemoveRange(histories);

        _context.Documents.Remove(document);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IncrementViewCountAsync(Guid id)
    {
        var document = await _context.Documents.FindAsync(id);
        if (document == null) return false;

        document.ViewCount++;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IncrementDownloadCountAsync(Guid id)
    {
        var document = await _context.Documents.FindAsync(id);
        if (document == null) return false;        document.DownloadCount++;
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region History Methods

    public async Task<UserDocumentHistory> AddHistoryAsync(UserDocumentHistory history)
    {
        history.Id = Guid.NewGuid();
        history.ViewedAt = DateTime.UtcNow;
        _context.UserDocumentHistories.Add(history);
        await _context.SaveChangesAsync();
        return history;
    }

    public async Task<IEnumerable<UserDocumentHistory>> GetUserHistoryAsync(Guid userId, int limit = 20)
    {
        return await _context.UserDocumentHistories
            .Include(h => h.Document)
                .ThenInclude(d => d.Category)
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.ViewedAt)
            .Take(limit)
            .ToListAsync();
    }

    #endregion

    #region Stats Methods

    public async Task<object> GetStatsAsync()
    {
        var totalDocuments = await _context.Documents.CountAsync();
        var totalCategories = await _context.DocumentCategories.CountAsync();
        var totalFileSize = await _context.Documents.SumAsync(d => (long?)d.FileSize) ?? 0;
        var totalViews = await _context.Documents.SumAsync(d => (int?)d.ViewCount) ?? 0;
        var totalDownloads = await _context.Documents.SumAsync(d => (int?)d.DownloadCount) ?? 0;

        return new
        {
            TotalDocuments = totalDocuments,
            TotalCategories = totalCategories,
            TotalFileSize = totalFileSize,
            TotalFileSizeFormatted = FormatFileSize(totalFileSize),
            TotalViews = totalViews,
            TotalDownloads = totalDownloads
        };
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    #endregion
}
