-- Fix Documents table by adding missing columns
-- This script adds all missing columns that should exist based on the entity model
-- Run this script on your database to fix the schema mismatch

USE [EnglishApp];
GO

PRINT '========================================';
PRINT 'Starting Documents table schema fix...';
PRINT '========================================';
GO

-- Add FileName column if it doesn't exist
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Documents]') 
    AND name = 'FileName'
)
BEGIN
    PRINT 'Adding FileName column to Documents table...';
    
    ALTER TABLE [dbo].[Documents]
    ADD [FileName] NVARCHAR(MAX) NOT NULL DEFAULT '';
    
    PRINT '✓ FileName column added successfully!';
END
ELSE
BEGIN
    PRINT '✓ FileName column already exists.';
END
GO

-- Add FileType column if it doesn't exist
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Documents]') 
    AND name = 'FileType'
)
BEGIN
    PRINT 'Adding FileType column to Documents table...';
    
    ALTER TABLE [dbo].[Documents]
    ADD [FileType] NVARCHAR(MAX) NOT NULL DEFAULT '';
    
    PRINT '✓ FileType column added successfully!';
END
ELSE
BEGIN
    PRINT '✓ FileType column already exists.';
END
GO

-- Add FileSize column if it doesn't exist
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Documents]') 
    AND name = 'FileSize'
)
BEGIN
    PRINT 'Adding FileSize column to Documents table...';
    
    ALTER TABLE [dbo].[Documents]
    ADD [FileSize] BIGINT NOT NULL DEFAULT 0;
    
    PRINT '✓ FileSize column added successfully!';
END
ELSE
BEGIN
    PRINT '✓ FileSize column already exists.';
END
GO

-- Add UploadedByUserId column if it doesn't exist
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Documents]') 
    AND name = 'UploadedByUserId'
)
BEGIN
    PRINT 'Adding UploadedByUserId column to Documents table...';
    
    ALTER TABLE [dbo].[Documents]
    ADD [UploadedByUserId] UNIQUEIDENTIFIER NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
    
    PRINT '✓ UploadedByUserId column added successfully!';
END
ELSE
BEGIN
    PRINT '✓ UploadedByUserId column already exists.';
END
GO

-- Add ViewCount column if it doesn't exist
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Documents]') 
    AND name = 'ViewCount'
)
BEGIN
    PRINT 'Adding ViewCount column to Documents table...';
    
    ALTER TABLE [dbo].[Documents]
    ADD [ViewCount] INT NOT NULL DEFAULT 0;
    
    PRINT '✓ ViewCount column added successfully!';
END
ELSE
BEGIN
    PRINT '✓ ViewCount column already exists.';
END
GO

-- Add DownloadCount column if it doesn't exist
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Documents]') 
    AND name = 'DownloadCount'
)
BEGIN
    PRINT 'Adding DownloadCount column to Documents table...';
    
    ALTER TABLE [dbo].[Documents]
    ADD [DownloadCount] INT NOT NULL DEFAULT 0;
    
    PRINT '✓ DownloadCount column added successfully!';
END
ELSE
BEGIN
    PRINT '✓ DownloadCount column already exists.';
END
GO

-- Add UpdatedAt column if it doesn't exist
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Documents]') 
    AND name = 'UpdatedAt'
)
BEGIN
    PRINT 'Adding UpdatedAt column to Documents table...';
    
    ALTER TABLE [dbo].[Documents]
    ADD [UpdatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE();
    
    PRINT '✓ UpdatedAt column added successfully!';
END
ELSE
BEGIN
    PRINT '✓ UpdatedAt column already exists.';
END
GO

-- Add foreign key constraint for UploadedByUserId if it doesn't exist
IF NOT EXISTS (
    SELECT * FROM sys.foreign_keys 
    WHERE name = 'FK_Documents_Users_UploadedByUserId'
    AND parent_object_id = OBJECT_ID(N'[dbo].[Documents]')
)
BEGIN
    PRINT 'Adding foreign key constraint FK_Documents_Users_UploadedByUserId...';
    
    -- First create an index if it doesn't exist
    IF NOT EXISTS (
        SELECT * FROM sys.indexes 
        WHERE name = 'IX_Documents_UploadedByUserId' 
        AND object_id = OBJECT_ID(N'[dbo].[Documents]')
    )
    BEGIN
        CREATE INDEX [IX_Documents_UploadedByUserId] 
        ON [dbo].[Documents]([UploadedByUserId]);
        PRINT '✓ Index IX_Documents_UploadedByUserId created.';
    END
    
    -- Add the foreign key constraint
    ALTER TABLE [dbo].[Documents]
    ADD CONSTRAINT [FK_Documents_Users_UploadedByUserId]
    FOREIGN KEY ([UploadedByUserId])
    REFERENCES [dbo].[Users] ([Id])
    ON DELETE NO ACTION;
    
    PRINT '✓ Foreign key constraint added successfully!';
END
ELSE
BEGIN
    PRINT '✓ Foreign key constraint already exists.';
END
GO

-- Verify the table structure
PRINT '';
PRINT '========================================';
PRINT 'Documents table structure:';
PRINT '========================================';
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE, 
    CHARACTER_MAXIMUM_LENGTH,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Documents'
ORDER BY ORDINAL_POSITION;
GO

PRINT '';
PRINT '========================================';
PRINT '✓ Documents table schema fix completed!';
PRINT '========================================';
GO
