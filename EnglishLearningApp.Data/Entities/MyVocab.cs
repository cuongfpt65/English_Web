using EnglishLearningApp.Data.Entities.User;

namespace EnglishLearningApp.Data.Entities.MyVocab
{
    /// <summary>
    /// Personal vocabulary - each user can have their own vocabulary collection
    /// </summary>
    public class MyVocab
    {
        public Guid Id { get; set; }
        
        /// <summary>
        /// User ID - to identify who owns this vocabulary
        /// </summary>
        public Guid UserId { get; set; }
        
        public string Word { get; set; } = "";
        public string Meaning { get; set; } = "";
        public string? Example { get; set; }
        public string? ImageUrl { get; set; }
        public string? Topic { get; set; }
        public string? Level { get; set; }
        
        /// <summary>
        /// Personal note for this vocabulary
        /// </summary>
        public string? Note { get; set; }
        
        /// <summary>
        /// Is this word marked as learned?
        /// </summary>
        public bool IsLearned { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual AppUser User { get; set; }
    }
}
