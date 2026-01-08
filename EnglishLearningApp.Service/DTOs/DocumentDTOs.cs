namespace EnglishLearningApp.Service.DTOs;

public class DocumentCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public int DocumentCount { get; set; }
}

public class CreateDocumentCategoryDto
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
}

public class DocumentDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string FileName { get; set; } = "";
    public string FileUrl { get; set; } = "";
    public string FileType { get; set; } = "";
    public long FileSize { get; set; }
    public string FileSizeFormatted { get; set; } = "";
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public Guid UploadedByUserId { get; set; }
    public string UploadedByName { get; set; } = "";
    public string UploadedByEmail { get; set; } = "";
    public int ViewCount { get; set; }
    public int DownloadCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateDocumentDto
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public Guid CategoryId { get; set; }
}

public class UpdateDocumentDto
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public Guid CategoryId { get; set; }
}

public class DocumentFilterDto
{
    public Guid? CategoryId { get; set; }
    public string? Search { get; set; }
    public string? FileType { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class DocumentStatsDto
{
    public int TotalDocuments { get; set; }
    public int TotalCategories { get; set; }
    public long TotalFileSize { get; set; }
    public string TotalFileSizeFormatted { get; set; } = "";
    public int TotalViews { get; set; }
    public int TotalDownloads { get; set; }
}
