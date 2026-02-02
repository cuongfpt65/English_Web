using EnglishLearningApp.Data.Entities.Document;
using EnglishLearningApp.Repository.Interfaces;
using EnglishLearningApp.Service.DTOs;
using EnglishLearningApp.Service.Interfaces;

namespace EnglishLearningApp.Service.Implementations;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileStorageService _fileStorageService;

    public DocumentService(IDocumentRepository documentRepository, IFileStorageService fileStorageService)
    {
        _documentRepository = documentRepository;
        _fileStorageService = fileStorageService;
    }

    #region Category Methods

    public async Task<IEnumerable<object>> GetAllCategoriesAsync()
    {
        var categories = await _documentRepository.GetAllCategoriesAsync();
        return categories.Select(c => new DocumentCategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            CreatedAt = c.CreatedAt,
            DocumentCount = c.Documents?.Count ?? 0
        });
    }

    public async Task<object?> GetCategoryByIdAsync(Guid id)
    {
        var category = await _documentRepository.GetCategoryByIdAsync(id);
        if (category == null) return null;

        return new DocumentCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            CreatedAt = category.CreatedAt,
            DocumentCount = category.Documents?.Count ?? 0
        };
    }

    public async Task<object> CreateCategoryAsync(object dto)
    {
        var createDto = (CreateDocumentCategoryDto)dto;
        
        var category = new DocumentCategory
        {
            Name = createDto.Name,
            Description = createDto.Description
        };

        category = await _documentRepository.CreateCategoryAsync(category);

        return new DocumentCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            CreatedAt = category.CreatedAt,
            DocumentCount = 0
        };
    }

    public async Task<object> UpdateCategoryAsync(Guid id, object dto)
    {
        var updateDto = (CreateDocumentCategoryDto)dto;
        var category = await _documentRepository.GetCategoryByIdAsync(id);
        
        if (category == null)
        {
            throw new Exception("Category not found");
        }

        category.Name = updateDto.Name;
        category.Description = updateDto.Description;

        category = await _documentRepository.UpdateCategoryAsync(category);

        return new DocumentCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            CreatedAt = category.CreatedAt,
            DocumentCount = category.Documents?.Count ?? 0
        };
    }    public async Task<bool> DeleteCategoryAsync(Guid id)
    {
        return await _documentRepository.DeleteCategoryAsync(id);
    }

    #endregion

    #region Document Methods

    public async Task<object> GetDocumentsAsync(Guid? categoryId = null, string? search = null, string? fileType = null, int page = 1, int pageSize = 10, Guid? uploaderId = null)
    {
        var (items, totalCount) = await _documentRepository.GetDocumentsAsync(categoryId, search, fileType, page, pageSize, uploaderId);

        var documents = items.Select(d => new DocumentDto
        {
            Id = d.Id,
            Title = d.Title,
            Description = d.Description,
            FileName = d.FileName,
            FileUrl = d.FileUrl,
            FileType = d.FileType,
            FileSize = d.FileSize,
            FileSizeFormatted = FormatFileSize(d.FileSize),
            CategoryId = d.CategoryId,
            CategoryName = d.Category?.Name ?? "",
            UploadedByUserId = d.UploadedByUserId,
            UploadedByName = d.UploadedBy?.FullName ?? "",
            UploadedByEmail = d.UploadedBy?.Email ?? "",
            ViewCount = d.ViewCount,
            DownloadCount = d.DownloadCount,
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt
        }).ToList();

        return new
        {
            Items = documents,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<object?> GetDocumentByIdAsync(Guid id)
    {
        var document = await _documentRepository.GetDocumentByIdAsync(id);
        if (document == null) return null;

        return new DocumentDto
        {
            Id = document.Id,
            Title = document.Title,
            Description = document.Description,
            FileName = document.FileName,
            FileUrl = document.FileUrl,
            FileType = document.FileType,
            FileSize = document.FileSize,
            FileSizeFormatted = FormatFileSize(document.FileSize),
            CategoryId = document.CategoryId,
            CategoryName = document.Category?.Name ?? "",
            UploadedByUserId = document.UploadedByUserId,
            UploadedByName = document.UploadedBy?.FullName ?? "",
            UploadedByEmail = document.UploadedBy?.Email ?? "",
            ViewCount = document.ViewCount,
            DownloadCount = document.DownloadCount,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt
        };
    }

    public async Task<object> UploadDocumentAsync(Guid userId, object dto, Stream fileStream, string fileName)
    {
        var createDto = (CreateDocumentDto)dto;        // Get file extension
        var fileExtension = Path.GetExtension(fileName).TrimStart('.').ToLower();
        
        // Validate file type - Only DOC and DOCX allowed
        var allowedTypes = new[] { "doc", "docx" };
        if (!allowedTypes.Contains(fileExtension))
        {
            throw new Exception($"File type not allowed. Only DOC and DOCX files are supported.");
        }

        // Get file size
        var fileSize = fileStream.Length;

        // Upload to Cloudinary
        var (fileUrl, publicId) = await _fileStorageService.UploadFileAsync(fileStream, fileName, "documents");

        // Create document record
        var document = new Document
        {
            Title = createDto.Title,
            Description = createDto.Description,
            FileName = fileName,
            FileUrl = fileUrl,
            FileType = fileExtension,
            FileSize = fileSize,
            CategoryId = createDto.CategoryId,
            UploadedByUserId = userId
        };

        document = await _documentRepository.CreateDocumentAsync(document);

        // Reload with relationships
        document = await _documentRepository.GetDocumentByIdAsync(document.Id);

        return new DocumentDto
        {
            Id = document!.Id,
            Title = document.Title,
            Description = document.Description,
            FileName = document.FileName,
            FileUrl = document.FileUrl,
            FileType = document.FileType,
            FileSize = document.FileSize,
            FileSizeFormatted = FormatFileSize(document.FileSize),
            CategoryId = document.CategoryId,
            CategoryName = document.Category?.Name ?? "",
            UploadedByUserId = document.UploadedByUserId,
            UploadedByName = document.UploadedBy?.FullName ?? "",
            UploadedByEmail = document.UploadedBy?.Email ?? "",
            ViewCount = document.ViewCount,
            DownloadCount = document.DownloadCount,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt
        };
    }

    public async Task<object> UpdateDocumentAsync(Guid id, Guid userId, object dto)
    {
        var updateDto = (UpdateDocumentDto)dto;
        var document = await _documentRepository.GetDocumentByIdAsync(id);

        if (document == null)
        {
            throw new Exception("Document not found");
        }

        // Check if user is the uploader
        if (document.UploadedByUserId != userId)
        {
            throw new Exception("You don't have permission to update this document");
        }

        document.Title = updateDto.Title;
        document.Description = updateDto.Description;
        document.CategoryId = updateDto.CategoryId;

        document = await _documentRepository.UpdateDocumentAsync(document);

        // Reload with relationships
        document = await _documentRepository.GetDocumentByIdAsync(document.Id);

        return new DocumentDto
        {
            Id = document!.Id,
            Title = document.Title,
            Description = document.Description,
            FileName = document.FileName,
            FileUrl = document.FileUrl,
            FileType = document.FileType,
            FileSize = document.FileSize,
            FileSizeFormatted = FormatFileSize(document.FileSize),
            CategoryId = document.CategoryId,
            CategoryName = document.Category?.Name ?? "",
            UploadedByUserId = document.UploadedByUserId,
            UploadedByName = document.UploadedBy?.FullName ?? "",
            UploadedByEmail = document.UploadedBy?.Email ?? "",
            ViewCount = document.ViewCount,
            DownloadCount = document.DownloadCount,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt
        };
    }

    public async Task<bool> DeleteDocumentAsync(Guid id, Guid userId)
    {
        var document = await _documentRepository.GetDocumentByIdAsync(id);

        if (document == null)
        {
            throw new Exception("Document not found");
        }

        // Check if user is the uploader (or you can add admin check)
        if (document.UploadedByUserId != userId)
        {
            throw new Exception("You don't have permission to delete this document");
        }

        // Delete from Cloudinary (extract public ID from URL)
        // Note: This is a simple extraction, adjust based on your URL structure
        var publicId = ExtractPublicIdFromUrl(document.FileUrl);
        if (!string.IsNullOrEmpty(publicId))
        {
            await _fileStorageService.DeleteFileAsync(publicId);
        }

        return await _documentRepository.DeleteDocumentAsync(id);
    }

    public async Task<bool> RecordViewAsync(Guid documentId, Guid userId)
    {
        await _documentRepository.IncrementViewCountAsync(documentId);
        
        var history = new UserDocumentHistory
        {
            UserId = userId,
            DocumentId = documentId,
            Action = "View"
        };

        await _documentRepository.AddHistoryAsync(history);
        return true;
    }

    public async Task<bool> RecordDownloadAsync(Guid documentId, Guid userId)
    {
        await _documentRepository.IncrementDownloadCountAsync(documentId);
        
        var history = new UserDocumentHistory
        {
            UserId = userId,
            DocumentId = documentId,
            Action = "Download"
        };

        await _documentRepository.AddHistoryAsync(history);
        return true;
    }

    #endregion

    #region History Methods

    public async Task<IEnumerable<object>> GetUserHistoryAsync(Guid userId, int limit = 20)
    {
        var history = await _documentRepository.GetUserHistoryAsync(userId, limit);
        
        return history.Select(h => new
        {
            h.Id,
            h.Action,
            h.ViewedAt,
            Document = new DocumentDto
            {
                Id = h.Document.Id,
                Title = h.Document.Title,
                Description = h.Document.Description,
                FileName = h.Document.FileName,
                FileUrl = h.Document.FileUrl,
                FileType = h.Document.FileType,
                FileSize = h.Document.FileSize,
                FileSizeFormatted = FormatFileSize(h.Document.FileSize),
                CategoryId = h.Document.CategoryId,
                CategoryName = h.Document.Category?.Name ?? "",
                ViewCount = h.Document.ViewCount,
                DownloadCount = h.Document.DownloadCount,
                CreatedAt = h.Document.CreatedAt
            }
        });
    }

    #endregion

    #region Stats Methods

    public async Task<object> GetStatsAsync()
    {
        return await _documentRepository.GetStatsAsync();
    }

    #endregion

    #region Helper Methods

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

    private string ExtractPublicIdFromUrl(string url)
    {
        try
        {
            // Cloudinary URL format: https://res.cloudinary.com/[cloud-name]/raw/upload/v[version]/[public-id]
            var uri = new Uri(url);
            var pathParts = uri.AbsolutePath.Split('/');
            
            // Find "upload" and get everything after version
            var uploadIndex = Array.IndexOf(pathParts, "upload");
            if (uploadIndex >= 0 && uploadIndex + 2 < pathParts.Length)
            {
                // Skip "upload" and version, join the rest
                var publicIdParts = pathParts.Skip(uploadIndex + 2).ToArray();
                return string.Join("/", publicIdParts);
            }
            
            return "";
        }
        catch
        {
            return "";
        }
    }

    #endregion
}
