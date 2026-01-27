-- Mark all existing migrations as applied
USE [EnglishApp];
GO

-- Ensure __EFMigrationsHistory table exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '__EFMigrationsHistory')
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END
GO

-- Mark InitialCreate as applied (skip table creation since they exist)
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251124121203_InitialCreate')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251124121203_InitialCreate', '9.0.10');
    PRINT 'Marked InitialCreate as applied';
END
GO

-- Mark AddDocumentFeatureEnhancement as applied if it exists
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251126155300_AddDocumentFeatureEnhancement')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251126155300_AddDocumentFeatureEnhancement', '9.0.10');
    PRINT 'Marked AddDocumentFeatureEnhancement as applied';
END
GO

-- Show all applied migrations
SELECT * FROM [__EFMigrationsHistory] ORDER BY [MigrationId];
GO
