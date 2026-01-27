# 💬 Tính năng Lưu Lịch Sử Chat Bot

## 📋 Tổng Quan

Hệ thống đã được cập nhật để **lưu toàn bộ lịch sử chat** giữa người dùng và chatbot. Mỗi người dùng có thể có nhiều phiên chat (sessions), và mỗi phiên chat lưu trữ tất cả tin nhắn trao đổi.

## 🗄️ Cấu Trúc Database

### Bảng `ChatSessions`
- `Id` (GUID) - Khóa chính
- `UserId` (GUID) - ID người dùng
- `Title` (string) - Tiêu đề phiên chat
- `CreatedAt` (DateTime) - Thời gian tạo

### Bảng `ChatMessages`
- `Id` (GUID) - Khóa chính
- `ChatSessionId` (GUID) - ID phiên chat
- `Sender` (string) - "User" hoặc "Bot"
- `Message` (string) - Nội dung tin nhắn
- `CreatedAt` (DateTime) - Thời gian gửi

## 🚀 API Endpoints

### 1. **Gửi Tin Nhắn Chat**
```http
POST /api/ChatBot
Content-Type: application/json
Authorization: Bearer {token}

{
  "type": "grammar_fix",
  "message": "I goes to school yesterday",
  "sessionId": "guid-optional"
}
```

**Response:**
```json
{
  "answer": "✔ **Câu đã sửa:**\nI went to school yesterday...",
  "sessionId": "12345678-1234-1234-1234-123456789abc"
}
```

### 2. **Tạo Phiên Chat Mới**
```http
POST /api/ChatBot/sessions
Content-Type: application/json
Authorization: Bearer {token}

{
  "title": "Grammar Practice Session"
}
```

**Response:**
```json
{
  "id": "12345678-1234-1234-1234-123456789abc",
  "title": "Grammar Practice Session",
  "createdAt": "2026-01-27T10:30:00Z"
}
```

### 3. **Lấy Danh Sách Phiên Chat**
```http
GET /api/ChatBot/sessions
Authorization: Bearer {token}
```

**Response:**
```json
[
  {
    "id": "12345678-1234-1234-1234-123456789abc",
    "title": "Grammar Practice",
    "createdAt": "2026-01-27T10:30:00Z"
  },
  {
    "id": "87654321-4321-4321-4321-cba987654321",
    "title": "Essay Writing",
    "createdAt": "2026-01-26T15:20:00Z"
  }
]
```

### 4. **Lấy Lịch Sử Chat của Một Phiên**
```http
GET /api/ChatBot/sessions/{sessionId}/messages
Authorization: Bearer {token}
```

**Response:**
```json
[
  {
    "id": "msg-1",
    "sender": "User",
    "message": "I goes to school yesterday",
    "createdAt": "2026-01-27T10:31:00Z"
  },
  {
    "id": "msg-2",
    "sender": "Bot",
    "message": "✔ **Câu đã sửa:**\nI went to school yesterday...",
    "createdAt": "2026-01-27T10:31:05Z"
  }
]
```

### 5. **Đổi Tên Phiên Chat**
```http
PUT /api/ChatBot/sessions/{sessionId}
Content-Type: application/json
Authorization: Bearer {token}

{
  "title": "Advanced Grammar Practice"
}
```

## 🔧 Cách Sử Dụng

### Frontend Flow

#### **1. Bắt Đầu Chat Mới**
```javascript
// Tạo session mới
const createSession = async () => {
  const response = await fetch('https://api.example.com/api/ChatBot/sessions', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify({
      title: 'New Chat Session'
    })
  });
  
  const session = await response.json();
  return session.id; // Lưu sessionId này
};
```

#### **2. Gửi Tin Nhắn**
```javascript
const sendMessage = async (sessionId, message, type) => {
  const response = await fetch('https://api.example.com/api/ChatBot', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify({
      type: type,
      message: message,
      sessionId: sessionId
    })
  });
  
  const result = await response.json();
  return result.answer;
};
```

#### **3. Hiển Thị Lịch Sử**
```javascript
const loadHistory = async (sessionId) => {
  const response = await fetch(
    `https://api.example.com/api/ChatBot/sessions/${sessionId}/messages`,
    {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    }
  );
  
  const messages = await response.json();
  // Hiển thị messages trong UI
  messages.forEach(msg => {
    console.log(`${msg.sender}: ${msg.message}`);
  });
};
```

#### **4. Danh Sách Phiên Chat**
```javascript
const loadSessions = async () => {
  const response = await fetch('https://api.example.com/api/ChatBot/sessions', {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  
  const sessions = await response.json();
  // Hiển thị danh sách sessions
  sessions.forEach(s => {
    console.log(`${s.title} - ${s.createdAt}`);
  });
};
```

## 📦 Các File Đã Thay Đổi

1. **Controllers/ChatBotController.cs** - Thêm endpoints mới
2. **Service/Implementations/ChatBotService.cs** - Lưu tin nhắn
3. **Repository/Interfaces/IChatRepository.cs** - Interface mới
4. **Repository/Implementations/ChatRepository.cs** - Implementation mới
5. **Program.cs** - Đăng ký IChatRepository
6. **CreateChatHistoryTables.sql** - Migration script

## 🛠️ Cài Đặt

### 1. Chạy Migration (nếu cần)
```sql
-- Chạy file CreateChatHistoryTables.sql trong database
```

### 2. Build và Chạy API
```bash
cd EnglishLearningApp.Api
dotnet build
dotnet run
```

## ✅ Lợi Ích

- ✔️ **Lưu toàn bộ lịch sử** chat của mỗi user
- ✔️ **Quản lý nhiều phiên** chat riêng biệt
- ✔️ **Xem lại** các cuộc hội thoại cũ
- ✔️ **Tùy chỉnh tên** phiên chat
- ✔️ **Theo dõi tiến độ** học tập qua lịch sử

## 🔒 Bảo Mật

- Tất cả endpoints yêu cầu **JWT Authentication**
- User chỉ xem được **phiên chat của chính mình**
- Không user nào truy cập được chat của user khác

## 📝 Lưu Ý

- Nếu không truyền `sessionId` trong request, hệ thống sẽ **tự động tạo** session mới
- Tin nhắn được lưu **tự động** mỗi khi có tương tác
- Sắp xếp sessions theo **thời gian tạo** (mới nhất trước)
- Sắp xếp messages theo **thời gian gửi** (cũ nhất trước)

## 🎯 Use Cases

### Case 1: User Chat Lần Đầu
1. User gửi message **không có sessionId**
2. Backend tự động tạo session mới
3. Lưu tin nhắn của user và bot
4. Trả về sessionId cho frontend

### Case 2: User Tiếp Tục Chat Cũ
1. User chọn session từ danh sách
2. Load lịch sử messages
3. Gửi message mới với sessionId
4. Tin nhắn được thêm vào session đó

### Case 3: User Xem Lại Lịch Sử
1. Load danh sách sessions
2. Chọn session muốn xem
3. Hiển thị tất cả messages của session đó

---

**Created by:** AI Assistant  
**Date:** January 27, 2026  
**Version:** 1.0
