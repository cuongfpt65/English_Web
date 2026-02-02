-- Mark top 20 vocabularies as learned for the first user (for testing AI Quiz)
-- This script helps you quickly set up test data for AI Quiz feature

DECLARE @UserId uniqueidentifier;

-- Get the first user in the system
SELECT TOP 1 @UserId = Id FROM Users ORDER BY CreatedAt;

PRINT 'Using User ID: ' + CAST(@UserId AS VARCHAR(50));

-- Insert UserVocabularies for top 20 vocabularies if not exists
INSERT INTO UserVocabularies (Id, UserId, VocabularyId, IsLearned, Note, CreatedAt)
SELECT 
    NEWID(),
    @UserId,
    v.Id,
    1, -- Mark as learned
    'Auto-marked for AI Quiz testing',
    GETUTCDATE()
FROM 
    (SELECT TOP 20 Id FROM Vocabularies ORDER BY NEWID()) v
WHERE 
    NOT EXISTS (
        SELECT 1 
        FROM UserVocabularies uv 
        WHERE uv.UserId = @UserId AND uv.VocabularyId = v.Id
    );

-- Update existing UserVocabularies to mark as learned
UPDATE UserVocabularies
SET IsLearned = 1,
    Note = CASE 
        WHEN Note IS NULL OR Note = '' THEN 'Marked for AI Quiz testing'
        ELSE Note 
    END
WHERE UserId = @UserId;

-- Show results
SELECT 
    u.Username,
    u.Email,
    COUNT(*) as LearnedCount
FROM Users u
LEFT JOIN UserVocabularies uv ON uv.UserId = u.Id AND uv.IsLearned = 1
WHERE u.Id = @UserId
GROUP BY u.Username, u.Email;

PRINT 'Done! You now have learned vocabularies for AI Quiz.';
