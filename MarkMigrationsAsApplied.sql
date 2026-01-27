-- Mark existing migrations as applied in __EFMigrationsHistory
USE [EnglishApp];
GO

-- Check if __EFMigrationsHistory table exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '__EFMigrationsHistory')
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
    PRINT '__EFMigrationsHistory table created.';
END

-- Insert migration records to mark them as applied
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251124121203_InitialCreate')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251124121203_InitialCreate', '9.0.10');
    PRINT 'Marked InitialCreate migration as applied.';
END
ELSE
BEGIN
    PRINT 'InitialCreate migration already marked as applied.';
END

-- Check for the new migration
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] LIKE '202%_AddDocumentFeatures')
BEGIN
    -- Get the actual migration name
    DECLARE @migrationId NVARCHAR(150);
    -- This will be filled after we know the exact migration name
    -- For now, we'll let the dotnet ef database update handle it
    PRINT 'Ready to apply new AddDocumentFeatures migration.';
END

SELECT * FROM [__EFMigrationsHistory] ORDER BY [MigrationId];
GO
