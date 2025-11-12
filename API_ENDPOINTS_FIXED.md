# API Endpoints - Fixed Issues

## 📋 Tóm tắt
Đã sửa các lỗi API 404 và 405 bằng cách thêm các endpoint còn thiếu vào backend.

---

## ✅ Vocabulary Controller - Fixed

### 1. GET `/api/vocabulary/user`
**Mô tả:** Lấy danh sách từ vựng của user hiện tại

**Response:**
```json
[
  {
    "id": "guid",
    "vocabularyId": "guid",
    "isLearned": true,
    "note": "My note"
  }
]
```

**Trạng thái:** ✅ ĐÃ THÊM

---

### 2. POST `/api/vocabulary/{vocabularyId}/toggle-learned`
**Mô tả:** Đánh dấu/bỏ đánh dấu từ vựng đã học

**Response:**
```json
{
  "message": "Status toggled successfully",
  "isLearned": true
}
```

**Trạng thái:** ✅ ĐÃ THÊM

---

### 3. POST `/api/vocabulary/{vocabularyId}/note`
**Mô tả:** Thêm ghi chú cho từ vựng

**Request Body:**
```json
{
  "note": "This is my note"
}
```

**Response:**
```json
{
  "message": "Note added successfully"
}
```

**Trạng thái:** ✅ ĐÃ THÊM

---

## ✅ Class Controller - Fixed

### 1. GET `/api/Class/{id}/members`
**Mô tả:** Lấy danh sách thành viên của lớp học

**Response:**
```json
[
  {
    "id": "guid",
    "userId": "guid",
    "userName": "username",
    "fullName": "Full Name",
    "role": "Student",
    "joinedAt": "2024-01-01T00:00:00Z",
    "vocabularyCount": 10,
    "bestQuizScore": 85.5
  }
]
```

**Trạng thái:** ✅ ĐÃ THÊM

---

### 2. DELETE `/api/Class/{classId}/members/{userId}`
**Mô tả:** Xóa thành viên khỏi lớp (chỉ teacher)

**Response:**
```json
{
  "message": "Member removed successfully"
}
```

**Trạng thái:** ✅ ĐÃ THÊM

---

### 3. DELETE `/api/Class/{id}/leave`
**Mô tả:** Rời khỏi lớp học (student)

**Response:**
```json
{
  "message": "Left class successfully"
}
```

**Trạng thái:** ✅ ĐÃ THÊM

---

## 🔧 Các thay đổi Backend

### VocabularyController.cs
```csharp
// Đã thêm:
[HttpGet("user")]                              // GET /api/vocabulary/user
[HttpPost("{vocabularyId}/toggle-learned")]    // POST /api/vocabulary/{id}/toggle-learned
[HttpPost("{vocabularyId}/note")]              // POST /api/vocabulary/{id}/note
```

### ClassController.cs
```csharp
// Đã thêm:
[HttpGet("{id}/members")]                      // GET /api/Class/{id}/members
[HttpDelete("{classId}/members/{userId}")]     // DELETE /api/Class/{classId}/members/{userId}
[HttpDelete("{id}/leave")]                     // DELETE /api/Class/{id}/leave
```

---

## 📝 Request/Response Models Added

### VocabularyController
```csharp
public class AddNoteRequest
{
    public string Note { get; set; } = "";
}
```

---

## 🧪 Testing

### Test Vocabulary Endpoints
```powershell
# 1. Get user vocabulary
curl -X GET "https://localhost:5019/api/vocabulary/user" `
  -H "Authorization: Bearer YOUR_TOKEN"

# 2. Toggle learned status
curl -X POST "https://localhost:5019/api/vocabulary/{vocabularyId}/toggle-learned" `
  -H "Authorization: Bearer YOUR_TOKEN"

# 3. Add note
curl -X POST "https://localhost:5019/api/vocabulary/{vocabularyId}/note" `
  -H "Authorization: Bearer YOUR_TOKEN" `
  -H "Content-Type: application/json" `
  -d '{"note":"My note"}'
```

### Test Class Endpoints
```powershell
# 1. Get class members
curl -X GET "https://localhost:5019/api/Class/{classId}/members" `
  -H "Authorization: Bearer YOUR_TOKEN"

# 2. Remove member (teacher only)
curl -X DELETE "https://localhost:5019/api/Class/{classId}/members/{userId}" `
  -H "Authorization: Bearer YOUR_TOKEN"

# 3. Leave class (student)
curl -X DELETE "https://localhost:5019/api/Class/{classId}/leave" `
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## ⚠️ Lưu ý quan trọng

1. **Authentication Required:** Tất cả các endpoint đều yêu cầu JWT token hợp lệ
2. **Authorization:**
   - Chỉ teacher mới có thể xóa members
   - Chỉ members mới có thể truy cập thông tin class
3. **Error Handling:**
   - 401: Unauthorized (không có token hoặc token hết hạn)
   - 403: Forbidden (không có quyền truy cập)
   - 404: Not Found (resource không tồn tại)
   - 500: Internal Server Error

---

## ✨ Next Steps

1. **Build & Run Backend:**
   ```powershell
   cd d:\Backup\App\English\EnglishLearningApp.Api
   dotnet build
   dotnet run
   ```

2. **Test Frontend:**
   ```powershell
   cd d:\Backup\App\English-app
   npm run dev
   ```

3. **Verify:** 
   - Đăng nhập vào app
   - Vào trang Lessons → kiểm tra danh sách vocabulary
   - Vào trang Class → kiểm tra danh sách members
   - Không còn lỗi 404 hoặc 405

---

## 📚 Related Documentation

- [QUIZ_FEATURE_GUIDE.md](../English-app/QUIZ_FEATURE_GUIDE.md) - Hướng dẫn tính năng Quiz
- [AUTHENTICATION_GUIDE.md](../English-app/AUTHENTICATION_GUIDE.md) - Hướng dẫn Authentication
- [API Documentation](./EnglishLearningApp.Api/README.md) - Tài liệu API đầy đủ

---

**Ngày cập nhật:** 2025-01-12  
**Version:** 1.0.0  
**Status:** ✅ Completed
