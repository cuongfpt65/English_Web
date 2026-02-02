using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EnglishLearningApp.Service.Interfaces;
using Microsoft.Extensions.Configuration;

namespace EnglishLearningApp.Service.Implementations;

public class CloudinaryFileStorageService : IFileStorageService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryFileStorageService(IConfiguration configuration)
    {
        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
    }    public async Task<(string Url, string PublicId)> UploadFileAsync(Stream fileStream, string fileName, string folder = "documents")
    {
        // Determine if file is an image based on file extension
        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
        var isImage = imageExtensions.Contains(fileExtension);

        UploadResult uploadResult;

        if (isImage && folder == "avatars")
        {
            // Upload as image with transformations for avatars
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = folder,
                PublicId = $"{folder}/{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension(fileName)}",
                Transformation = new Transformation()
                    .Width(500)
                    .Height(500)
                    .Crop("fill")
                    .Quality("auto")
                    .FetchFormat("auto")
            };

            uploadResult = await _cloudinary.UploadAsync(uploadParams);
        }
        else
        {
            // Upload all other files as raw resources (documents)
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = folder,
                PublicId = $"{folder}/{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension(fileName)}"
            };

            uploadResult = await _cloudinary.UploadAsync(uploadParams);
        }

        if (uploadResult.Error != null)
        {
            throw new Exception($"File upload failed: {uploadResult.Error.Message}");
        }

        Console.WriteLine($"=== File Upload Success ===");
        Console.WriteLine($"File: {fileName}");
        Console.WriteLine($"URL: {uploadResult.SecureUrl}");
        Console.WriteLine($"PublicId: {uploadResult.PublicId}");

        return (uploadResult.SecureUrl.ToString(), uploadResult.PublicId);
    }    public async Task<bool> DeleteFileAsync(string publicId)
    {
        try
        {
            // Try deleting as image first
            var imageDeleteParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image
            };
            var imageResult = await _cloudinary.DestroyAsync(imageDeleteParams);
            
            if (imageResult.Result == "ok")
            {
                return true;
            }

            // If not found as image, try as raw resource
            var rawDeleteParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Raw
            };
            var rawResult = await _cloudinary.DestroyAsync(rawDeleteParams);
            
            return rawResult.Result == "ok";
        }
        catch
        {
            return false;
        }
    }public string GetFileUrl(string publicId)
    {
        // For raw files (documents), we need to use the raw resource type URL
        // The publicId already contains the full path with folder
        return $"https://res.cloudinary.com/{_cloudinary.Api.Account.Cloud}/raw/upload/{publicId}";
    }
}
