# Fix Cloudinary PDF Authentication Error (401)

## 🚨 Vấn đề
- File PDF upload lên Cloudinary bị lỗi **401 Unauthorized** khi truy cập
- File DOC/DOCX hoạt động bình thường
- URL trả về nhưng không thể xem được

## 🔍 Nguyên nhân
Cloudinary có các **Resource Type** khác nhau:
1. **Image** - cho hình ảnh, PDF (khi upload như image)
2. **Video** - cho video
3. **Raw** - cho các file khác (DOC, DOCX, ZIP, etc.)

PDF có thể được upload theo 2 cách:
- **Như Image** (format=pdf) → Dễ truy cập, hỗ trợ inline viewing
- **Như Raw** → Cần authentication, dễ bị 401

## ✅ Giải pháp đã áp dụng

### 1. Upload PDF như Image Resource
```csharp
if (fileExtension == "pdf")
{
    var imageUploadParams = new ImageUploadParams
    {
        File = new FileDescription(fileName, fileStream),
        Folder = folder,
        PublicId = $"{folder}/{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension(fileName)}",
        Format = "pdf",                    // ✅ Chỉ định format là PDF
        ResourceType = ResourceType.Image, // ✅ Upload như image
        Type = "upload",
        Overwrite = false
    };
}
```

### 2. Giữ nguyên Raw Upload cho DOC/DOCX
```csharp
else
{
    var uploadParams = new RawUploadParams
    {
        File = new FileDescription(fileName, fileStream),
        Folder = folder,
        PublicId = $"{folder}/{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension(fileName)}",
        AccessMode = "public"
    };
}
```

## 🧪 Cách test

### Bước 1: Xóa file PDF cũ
1. Vào Cloudinary Dashboard: https://console.cloudinary.com/
2. Media Library → Tìm file PDF cũ (có lỗi 401)
3. Xóa file đó

### Bước 2: Restart Backend
```powershell
# Stop backend nếu đang chạy
# Restart lại
```

### Bước 3: Upload file PDF mới
1. Login với tài khoản Teacher/Admin
2. Vào trang "Quản lý Tài liệu"
3. Upload file PDF mới
4. Click "Xem" → PDF sẽ hiển thị inline

### Bước 4: Kiểm tra URL
URL mới sẽ có dạng:
```
https://res.cloudinary.com/{cloud}/image/upload/documents/documents/{id}.pdf
```
(Chú ý: `/image/` thay vì `/raw/`)

## 🔧 Alternative: Nếu vẫn lỗi

### Option 1: Sử dụng Signed URL
Thêm vào `CloudinaryFileStorageService.cs`:

```csharp
private string GenerateSignedUrl(string publicId, string resourceType = "raw")
{
    var parameters = new SortedDictionary<string, object>
    {
        { "timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
    };
    
    var url = _cloudinary.Api.UrlImgUp
        .ResourceType(resourceType)
        .Signed(true)
        .BuildUrl(publicId);
    
    return url;
}
```

### Option 2: Disable Authentication trong Cloudinary
1. Vào Settings → Security
2. Tìm "Restricted media types"
3. Bỏ check "Restrict access to media"

### Option 3: Sử dụng Google Drive hoặc AWS S3
Nếu Cloudinary không phù hợp, có thể chuyển sang:
- Google Drive API
- AWS S3 + CloudFront
- Azure Blob Storage

## 📊 So sánh Resource Types

| Feature | Image (PDF) | Raw (PDF) |
|---------|-------------|-----------|
| Upload dễ dàng | ✅ | ✅ |
| Inline viewing | ✅ | ⚠️ Có thể bị 401 |
| Public access | ✅ | ❌ Cần config |
| Transformations | ✅ | ❌ |
| Best for | Web viewing | Download |

## 🎯 Kết luận
- **Upload PDF như Image** là cách đơn giản và hiệu quả nhất
- DOC/DOCX vẫn dùng Raw upload vì chúng hoạt động tốt
- Nếu cần bảo mật cao hơn, sử dụng Signed URLs
