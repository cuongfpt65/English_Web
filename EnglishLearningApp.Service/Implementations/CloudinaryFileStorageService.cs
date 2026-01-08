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
        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = folder,
            PublicId = $"{folder}/{Guid.NewGuid()}_{fileName}"
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
        {
            throw new Exception($"File upload failed: {uploadResult.Error.Message}");
        }

        return (uploadResult.SecureUrl.ToString(), uploadResult.PublicId);
    }

    public async Task<bool> DeleteFileAsync(string publicId)
    {
        try
        {
            var deleteParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Raw
            };

            var result = await _cloudinary.DestroyAsync(deleteParams);
            return result.Result == "ok";
        }
        catch
        {
            return false;
        }
    }

    public string GetFileUrl(string publicId)
    {
        return _cloudinary.Api.UrlImgUp.BuildUrl(publicId);
    }
}
