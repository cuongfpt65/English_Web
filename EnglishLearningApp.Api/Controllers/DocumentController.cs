using EnglishLearningApp.Service.DTOs;
using EnglishLearningApp.Service.Interfaces;
using EnglishLearningApp.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EnglishLearningApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class DocumentController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string GetUserRole() => User.FindFirstValue(ClaimTypes.Role)!;

    #region Category Endpoints

    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var categories = await _documentService.GetAllCategoriesAsync();
            return Ok(new { success = true, data = categories });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("categories/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategoryById(Guid id)
    {
        try
        {
            var category = await _documentService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound(new { success = false, message = "Category not found" });
            }
            return Ok(new { success = true, data = category });
        }
        catch (Exception ex)
        {            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("categories")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateDocumentCategoryDto dto)
    {
        try
        {
            var category = await _documentService.CreateCategoryAsync(dto);
            return Ok(new { success = true, data = category });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPut("categories/{id}")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] CreateDocumentCategoryDto dto)
    {
        try
        {
            var category = await _documentService.UpdateCategoryAsync(id, dto);
            return Ok(new { success = true, data = category });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpDelete("categories/{id}")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        try
        {
            var result = await _documentService.DeleteCategoryAsync(id);
            if (!result)
            {
                return NotFound(new { success = false, message = "Category not found" });
            }
            return Ok(new { success = true, message = "Category deleted successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region Document Endpoints

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetDocuments(
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? search = null,
        [FromQuery] string? fileType = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _documentService.GetDocumentsAsync(categoryId, search, fileType, page, pageSize);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDocumentById(Guid id)
    {
        try
        {
            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null)
            {
                return NotFound(new { success = false, message = "Document not found" });
            }
            return Ok(new { success = true, data = document });
        }
        catch (Exception ex)
        {            return BadRequest(new { success = false, message = ex.Message });
        }
    }    [HttpPost("upload")]
    [Authorize(Roles = "Teacher,Admin")]
    [RequestSizeLimit(52428800)] // 50MB
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadDocument([FromForm] DocumentUploadDto uploadDto)
    {
        try
        {
            Console.WriteLine("=== UPLOAD DOCUMENT ENDPOINT DEBUG ===");
            Console.WriteLine($"Request ContentType: {Request.ContentType}");
            Console.WriteLine($"Has Form Files: {Request.Form.Files.Count}");
            Console.WriteLine($"Form Keys: {string.Join(", ", Request.Form.Keys)}");
            
            if (Request.Form.Files.Count > 0)
            {
                var file = Request.Form.Files[0];
                Console.WriteLine($"First File - Name: {file.Name}, FileName: {file.FileName}, Length: {file.Length}");
            }
            
            Console.WriteLine($"UploadDto - Title: {uploadDto.Title}");
            Console.WriteLine($"UploadDto - Description: {uploadDto.Description}");
            Console.WriteLine($"UploadDto - CategoryId: {uploadDto.CategoryId}");
            Console.WriteLine($"UploadDto - File: {uploadDto.File?.FileName ?? "NULL"}");
            
            if (uploadDto.File == null || uploadDto.File.Length == 0)
            {
                Console.WriteLine("ERROR: File is null or empty");
                return BadRequest(new { success = false, message = "File is required" });
            }

            // Validate file size (50MB max)
            if (uploadDto.File.Length > 52428800)
            {
                return BadRequest(new { success = false, message = "File size must not exceed 50MB" });
            }

            var dto = new CreateDocumentDto
            {
                Title = uploadDto.Title,
                Description = uploadDto.Description,
                CategoryId = uploadDto.CategoryId
            };

            using var stream = uploadDto.File.OpenReadStream();
            var userId = GetUserId();
            Console.WriteLine($"UserId: {userId}");
            
            var document = await _documentService.UploadDocumentAsync(userId, dto, stream, uploadDto.File.FileName);

            Console.WriteLine($"Upload successful! Document ID: ");
            return Ok(new { success = true, data = document, message = "Document uploaded successfully" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR in UploadDocument: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> UpdateDocument(Guid id, [FromBody] UpdateDocumentDto dto)
    {
        try
        {
            var userId = GetUserId();
            var document = await _documentService.UpdateDocumentAsync(id, userId, dto);
            return Ok(new { success = true, data = document, message = "Document updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> DeleteDocument(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var result = await _documentService.DeleteDocumentAsync(id, userId);
            
            if (!result)
            {
                return NotFound(new { success = false, message = "Document not found" });
            }

            return Ok(new { success = true, message = "Document deleted successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }    [HttpPost("{id}/view")]
    [Authorize]
    public async Task<IActionResult> RecordView(Guid id)
    {
        try
        {
            var userId = GetUserId();
            await _documentService.RecordViewAsync(id, userId);
            return Ok(new { success = true, message = "View recorded" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("{id}/download")]
    [Authorize]
    public async Task<IActionResult> RecordDownload(Guid id)
    {
        try
        {
            var userId = GetUserId();
            await _documentService.RecordDownloadAsync(id, userId);
            return Ok(new { success = true, message = "Download recorded" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region History Endpoints

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] int limit = 20)
    {
        try
        {
            var userId = GetUserId();
            var history = await _documentService.GetUserHistoryAsync(userId, limit);
            return Ok(new { success = true, data = history });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region Stats Endpoints

    [HttpGet("stats")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            var stats = await _documentService.GetStatsAsync();
            return Ok(new { success = true, data = stats });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    #endregion
}
