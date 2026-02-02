-- Update all existing users to have IsActive = true
-- This ensures that existing users are not locked out after adding the IsActive column

UPDATE Users
SET IsActive = 1
WHERE IsActive = 0;

-- Verify the update
SELECT 
    Id,
    Email,
    FullName,
    Role,
    IsActive,
    CreatedAt
FROM Users
ORDER BY CreatedAt DESC;
