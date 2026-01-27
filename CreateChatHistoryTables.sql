-- Migration: Tạo bảng ChatSessions và ChatMessages nếu chưa có

-- Kiểm tra và tạo bảng ChatSessions
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ChatSessions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ChatSessions] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [UserId] UNIQUEIDENTIFIER NOT NULL,
        [Title] NVARCHAR(500) NOT NULL DEFAULT '',
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [FK_ChatSessions_Users] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id])
    );
    
    CREATE INDEX [IX_ChatSessions_UserId] ON [ChatSessions]([UserId]);
    CREATE INDEX [IX_ChatSessions_CreatedAt] ON [ChatSessions]([CreatedAt] DESC);
    
    PRINT 'Table ChatSessions created successfully.';
END
ELSE
BEGIN
    PRINT 'Table ChatSessions already exists.';
END
GO

-- Kiểm tra và tạo bảng ChatMessages
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ChatMessages]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ChatMessages] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [ChatSessionId] UNIQUEIDENTIFIER NOT NULL,
        [Sender] NVARCHAR(50) NOT NULL DEFAULT 'User',
        [Message] NVARCHAR(MAX) NOT NULL DEFAULT '',
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [FK_ChatMessages_ChatSessions] FOREIGN KEY ([ChatSessionId]) REFERENCES [ChatSessions]([Id])
    );
    
    CREATE INDEX [IX_ChatMessages_ChatSessionId] ON [ChatMessages]([ChatSessionId]);
    CREATE INDEX [IX_ChatMessages_CreatedAt] ON [ChatMessages]([CreatedAt]);
    
    PRINT 'Table ChatMessages created successfully.';
END
ELSE
BEGIN
    PRINT 'Table ChatMessages already exists.';
END
GO

PRINT 'Migration completed successfully!';
