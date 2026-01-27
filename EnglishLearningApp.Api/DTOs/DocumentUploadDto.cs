using Microsoft.AspNetCore.Http;

namespace EnglishLearningApp.Api.DTOs;

public class DocumentUploadDto
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public Guid CategoryId { get; set; }
    public IFormFile File { get; set; } = null!;
}
