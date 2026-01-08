-- Fix Duplicate Column in Documents Table
-- This script removes the duplicate 'UploadedById' column
-- and keeps only 'UploadedByUserId'

USE [EnglishApp];
GO

-- Step 1: Drop the foreign key constraint
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Documents_Users_UploadedById')
BEGIN
    ALTER TABLE [Documents] DROP CONSTRAINT [FK_Documents_Users_UploadedById];
    PRINT 'Dropped FK_Documents_Users_UploadedById';
END

-- Step 2: Drop the index
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Documents_UploadedById' AND object_id = OBJECT_ID('Documents'))
BEGIN
    DROP INDEX [IX_Documents_UploadedById] ON [Documents];
    PRINT 'Dropped IX_Documents_UploadedById';
END

-- Step 3: Drop the duplicate column
IF EXISTS (SELECT * FROM sys.columns WHERE name = 'UploadedById' AND object_id = OBJECT_ID('Documents'))
BEGIN
    ALTER TABLE [Documents] DROP COLUMN [UploadedById];
    PRINT 'Dropped UploadedById column';
END

PRINT 'Document table cleanup completed successfully!';
GO
