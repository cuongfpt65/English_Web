-- Fix for duplicate Action column in UserDocumentHistories table
-- This script will:
-- 1. Check if Action column already exists
-- 2. Mark migrations as applied to prevent re-running them
-- 3. Ensure database state matches expected schema

USE [EnglishLearningDb];
GO

-- Step 1: Check current state
PRINT '=== Checking Current Database State ===';
GO

-- Check if Action column exists
IF EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'UserDocumentHistories' 
    AND COLUMN_NAME = 'Action'
)
BEGIN
    PRINT 'Action column already exists in UserDocumentHistories table';
END
ELSE
BEGIN
    PRINT 'Action column does NOT exist in UserDocumentHistories table';
    -- Add it if it doesn't exist
    ALTER TABLE UserDocumentHistories
    ADD [Action] nvarchar(50) NOT NULL DEFAULT 'View';
    PRINT 'Action column added successfully';
END
GO

-- Step 2: Ensure __EFMigrationsHistory table exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '__EFMigrationsHistory')
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
    PRINT '__EFMigrationsHistory table created';
END
GO

-- Step 3: Mark all migrations as applied
PRINT '=== Marking Migrations as Applied ===';
GO

-- List of all migrations in chronological order
DECLARE @migrations TABLE (MigrationId nvarchar(150), ProductVersion nvarchar(32));

INSERT INTO @migrations VALUES 
    ('20251124121203_InitialCreate', '8.0.0'),
    ('20251125115423_updateData', '8.0.0'),
    ('20251125120714_update', '8.0.0'),
    ('20251125122558_updte', '8.0.0'),
    ('20251126151345_AddPasswordResetToken', '8.0.0'),
    ('20251126155300_AddDocumentFeatureEnhancement', '8.0.0'),
    ('20260127002134_AddDocumentFeatures', '8.0.0'),
    ('20260127030547_AddEmailVerificationAndEmailConfirmed', '8.0.0'),
    ('20260127045843_AddActionColumnToUserDocumentHistories', '8.0.0');

-- Insert migrations that don't already exist
MERGE INTO __EFMigrationsHistory AS target
USING @migrations AS source
ON target.MigrationId = source.MigrationId
WHEN NOT MATCHED THEN
    INSERT (MigrationId, ProductVersion)
    VALUES (source.MigrationId, source.ProductVersion);

PRINT 'All migrations marked as applied';
GO

-- Step 4: Verify the Action column has correct definition
PRINT '=== Verifying Action Column Definition ===';
GO

SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'UserDocumentHistories'
AND COLUMN_NAME = 'Action';
GO

-- Step 5: Show all applied migrations
PRINT '=== Applied Migrations ===';
SELECT MigrationId, ProductVersion 
FROM __EFMigrationsHistory 
ORDER BY MigrationId;
GO

PRINT '=== Fix Complete ===';
PRINT 'You can now run dotnet ef database update without errors';
GO
