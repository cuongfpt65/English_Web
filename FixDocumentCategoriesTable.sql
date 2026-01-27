-- Fix DocumentCategories table by adding missing CreatedAt column
-- Run this script on your database to fix the schema mismatch

USE [EnglishApp];
GO

-- Check if CreatedAt column exists
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[DocumentCategories]') 
    AND name = 'CreatedAt'
)
BEGIN
    PRINT 'Adding CreatedAt column to DocumentCategories table...';
    
    ALTER TABLE [dbo].[DocumentCategories]
    ADD [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE();
    
    PRINT 'CreatedAt column added successfully!';
END
ELSE
BEGIN
    PRINT 'CreatedAt column already exists in DocumentCategories table.';
END
GO

-- Verify the table structure
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'DocumentCategories'
ORDER BY ORDINAL_POSITION;
GO

PRINT 'DocumentCategories table structure verified!';
GO
