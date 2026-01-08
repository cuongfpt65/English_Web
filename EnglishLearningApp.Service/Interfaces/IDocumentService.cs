namespace EnglishLearningApp.Service.Interfaces;

public interface IDocumentService
{
    // Category
    Task<IEnumerable<object>> GetAllCategoriesAsync();
    Task<object?> GetCategoryByIdAsync(Guid id);
    Task<object> CreateCategoryAsync(object dto);
    Task<object> UpdateCategoryAsync(Guid id, object dto);
    Task<bool> DeleteCategoryAsync(Guid id);

    // Document
    Task<object> GetDocumentsAsync(Guid? categoryId = null, string? search = null, string? fileType = null, int page = 1, int pageSize = 10);
    Task<object?> GetDocumentByIdAsync(Guid id);
    Task<object> UploadDocumentAsync(Guid userId, object dto, Stream fileStream, string fileName);
    Task<object> UpdateDocumentAsync(Guid id, Guid userId, object dto);
    Task<bool> DeleteDocumentAsync(Guid id, Guid userId);
    Task<bool> RecordViewAsync(Guid documentId, Guid userId);
    Task<bool> RecordDownloadAsync(Guid documentId, Guid userId);

    // History
    Task<IEnumerable<object>> GetUserHistoryAsync(Guid userId, int limit = 20);

    // Stats
    Task<object> GetStatsAsync();
}

public interface IFileStorageService
{
    Task<(string Url, string PublicId)> UploadFileAsync(Stream fileStream, string fileName, string folder = "documents");
    Task<bool> DeleteFileAsync(string publicId);
    string GetFileUrl(string publicId);
}
