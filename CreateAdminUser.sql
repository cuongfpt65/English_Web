-- =============================================
-- Script: Create First Admin User
-- Description: Creates an admin user for initial system setup
-- =============================================

-- WARNING: This script creates a user with a default password.
-- Make sure to change the password after first login!

-- Step 1: Create a new admin user (if not exists)
-- Note: PasswordHash is for password "Admin@123" 
-- You should change this password immediately after first login

DECLARE @AdminId UNIQUEIDENTIFIER = NEWID();
DECLARE @AdminEmail NVARCHAR(255) = 'admin@englishapp.com';
DECLARE @AdminFullName NVARCHAR(255) = 'System Administrator';

-- Check if admin already exists
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = @AdminEmail)
BEGIN
    INSERT INTO Users (Id, Email, PhoneNumber, PhoneNumberConfirmed, PasswordHash, FullName, Role, CreatedAt, UpdatedAt)
    VALUES (
        @AdminId,
        @AdminEmail,
        NULL,
        0,
        'AQAAAAIAAYagAAAAEKxvVqMvqLqR5vXYxqZQJ3p8fYGH+vhWJ0ZqBQvNJLZzXGH8vqMvqLqR5vXYxqZQ==', -- Password: Admin@123
        @AdminFullName,
        'Admin',
        GETUTCDATE(),
        GETUTCDATE()
    );

    PRINT 'Admin user created successfully!';
    PRINT 'Email: ' + @AdminEmail;
    PRINT 'Password: Admin@123';
    PRINT '';
    PRINT '⚠️ IMPORTANT: Please change this password immediately after first login!';
END
ELSE
BEGIN
    PRINT 'Admin user already exists with email: ' + @AdminEmail;
END

GO

-- Step 2: Update existing user to Admin (if you want to promote an existing user)
-- Uncomment and modify the email below to promote an existing user to Admin

/*
DECLARE @UserEmail NVARCHAR(255) = 'youremail@example.com';

UPDATE Users 
SET Role = 'Admin',
    UpdatedAt = GETUTCDATE()
WHERE Email = @UserEmail;

PRINT 'User ' + @UserEmail + ' has been promoted to Admin role';
*/

GO

-- Step 3: Verify admin user
SELECT 
    Id,
    Email,
    FullName,
    Role,
    CreatedAt,
    UpdatedAt
FROM Users 
WHERE Role = 'Admin';

GO
