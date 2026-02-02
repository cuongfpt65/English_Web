-- ====================================
-- 🔍 Script Kiểm Tra Documents Hiện Tại
-- ====================================

-- Xem tất cả tài liệu đã upload
SELECT 
    Id,
    Title,
    FileName,
    FileUrl,
    FileSize,
    CASE 
        WHEN FileUrl LIKE '%cloudinary%' THEN '☁️ Cloudinary'
        WHEN FileUrl LIKE '%localhost%' THEN '📁 Local'
        ELSE '❓ Unknown'
    END AS StorageType,
    UploadedAt,
    (SELECT FullName FROM Users WHERE Id = Documents.UploadedByUserId) AS UploadedBy
FROM Documents
ORDER BY UploadedAt DESC;

-- Đếm số lượng theo loại storage
SELECT 
    CASE 
        WHEN FileUrl LIKE '%cloudinary%' THEN 'Cloudinary'
        WHEN FileUrl LIKE '%localhost%' THEN 'Local'
        ELSE 'Unknown'
    END AS StorageType,
    COUNT(*) AS TotalDocuments,
    SUM(FileSize) AS TotalSize_Bytes,
    CAST(SUM(FileSize) / 1024.0 / 1024.0 AS DECIMAL(10,2)) AS TotalSize_MB
FROM Documents
GROUP BY 
    CASE 
        WHEN FileUrl LIKE '%cloudinary%' THEN 'Cloudinary'
        WHEN FileUrl LIKE '%localhost%' THEN 'Local'
        ELSE 'Unknown'
    END;

-- Xem 5 file mới nhất
SELECT TOP 5
    Title,
    FileName,
    FileUrl,
    FileSize,
    UploadedAt
FROM Documents
ORDER BY UploadedAt DESC;
