using EnglishLearningApp.Service.Interfaces;
using Microsoft.Extensions.Configuration;

namespace EnglishLearningApp.Service.Implementations;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadPath;
    private readonly string _baseUrl;    public LocalFileStorageService(IConfiguration configuration)
    {
        // Lấy path từ configuration hoặc dùng default
        var contentRoot = configuration["ContentRootPath"] ?? Directory.GetCurrentDirectory();
        _uploadPath = Path.Combine(contentRoot, "wwwroot", "uploads", "documents");
        _baseUrl = configuration["AppSettings:BaseUrl"] ?? "http://localhost:5000";

        Console.WriteLine($"=== LocalFileStorageService Configuration ===");
        Console.WriteLine($"ContentRoot: {contentRoot}");
        Console.WriteLine($"Upload Path: {_uploadPath}");
        Console.WriteLine($"Base URL: {_baseUrl}");
        Console.WriteLine($"Upload Path Exists: {Directory.Exists(_uploadPath)}");

        // Tạo thư mục nếu chưa có
        if (!Directory.Exists(_uploadPath))
        {
            Console.WriteLine($"Creating upload directory: {_uploadPath}");
            Directory.CreateDirectory(_uploadPath);
        }
    }    public async Task<(string Url, string PublicId)> UploadFileAsync(Stream fileStream, string fileName, string folder = "documents")
    {
        try
        {
            Console.WriteLine($"=== UploadFileAsync ===");
            Console.WriteLine($"FileName: {fileName}");
            Console.WriteLine($"Folder: {folder}");
            Console.WriteLine($"FileStream Length: {fileStream.Length}");
            Console.WriteLine($"FileStream CanRead: {fileStream.CanRead}");

            // Tạo tên file unique
            var fileExtension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var subFolder = Path.Combine(_uploadPath, folder);

            Console.WriteLine($"Unique FileName: {uniqueFileName}");
            Console.WriteLine($"SubFolder: {subFolder}");

            // Tạo subfolder nếu cần
            if (!Directory.Exists(subFolder))
            {
                Console.WriteLine($"Creating subfolder: {subFolder}");
                Directory.CreateDirectory(subFolder);
            }

            var filePath = Path.Combine(subFolder, uniqueFileName);
            Console.WriteLine($"Full file path: {filePath}");

            // Lưu file
            using (var fileStreamOutput = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamOutput);
            }

            Console.WriteLine($"File saved successfully at: {filePath}");

            // Tạo URL
            var fileUrl = $"{_baseUrl}/uploads/documents/{folder}/{uniqueFileName}";
            var publicId = $"{folder}/{uniqueFileName}";

            Console.WriteLine($"File URL: {fileUrl}");
            Console.WriteLine($"Public ID: {publicId}");

            return (fileUrl, publicId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR in UploadFileAsync: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw new Exception($"File upload failed: {ex.Message}", ex);
        }
    }

    public Task<bool> DeleteFileAsync(string publicId)
    {
        try
        {
            var filePath = Path.Combine(_uploadPath, publicId.Replace("/", Path.DirectorySeparatorChar.ToString()));
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public string GetFileUrl(string publicId)
    {
        return $"{_baseUrl}/uploads/documents/{publicId}";
    }
}
