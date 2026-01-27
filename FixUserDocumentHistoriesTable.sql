-- Fix UserDocumentHistories table by adding the missing Action column
-- This column is defined in the entity model but was missing from the initial migration

-- Check if the column already exists before adding it
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'UserDocumentHistories' 
    AND COLUMN_NAME = 'Action'
)
BEGIN
    PRINT 'Adding Action column to UserDocumentHistories table...'
    
    -- Add the Action column with default value
    ALTER TABLE [UserDocumentHistories]
    ADD [Action] NVARCHAR(50) NOT NULL DEFAULT 'View';
    
    PRINT 'Action column added successfully!'
END
ELSE
BEGIN
    PRINT 'Action column already exists in UserDocumentHistories table.'
END

-- Update any existing records to have proper Action values
-- Set default to 'View' for all existing records
UPDATE [UserDocumentHistories]
SET [Action] = 'View'
WHERE [Action] IS NULL OR [Action] = '';

PRINT 'UserDocumentHistories table fixed successfully!'

-- Verify the fix
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'UserDocumentHistories'
ORDER BY ORDINAL_POSITION;
