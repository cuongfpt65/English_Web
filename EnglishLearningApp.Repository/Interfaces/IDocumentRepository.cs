using EnglishLearningApp.Data.Entities.Document;

namespace EnglishLearningApp.Repository.Interfaces;

public interface IDocumentRepository
{
    // Category
    Task<IEnumerable<DocumentCategory>> GetAllCategoriesAsync();
    Task<DocumentCategory?> GetCategoryByIdAsync(Guid id);
    Task<DocumentCategory> CreateCategoryAsync(DocumentCategory category);
    Task<DocumentCategory> UpdateCategoryAsync(DocumentCategory category);    Task<bool> DeleteCategoryAsync(Guid id);

    // Document
    Task<(IEnumerable<Document> Items, int TotalCount)> GetDocumentsAsync(
        Guid? categoryId = null, 
        string? search = null, 
        string? fileType = null,
        int page = 1, 
        int pageSize = 10,
        Guid? uploaderId = null);
    
    Task<Document?> GetDocumentByIdAsync(Guid id);
    Task<Document> CreateDocumentAsync(Document document);
    Task<Document> UpdateDocumentAsync(Document document);
    Task<bool> DeleteDocumentAsync(Guid id);
    Task<bool> IncrementViewCountAsync(Guid id);
    Task<bool> IncrementDownloadCountAsync(Guid id);

    // History
    Task<UserDocumentHistory> AddHistoryAsync(UserDocumentHistory history);
    Task<IEnumerable<UserDocumentHistory>> GetUserHistoryAsync(Guid userId, int limit = 20);
    
    // Stats
    Task<object> GetStatsAsync();
}
