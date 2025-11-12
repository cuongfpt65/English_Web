using EnglishLearningApp.Data.Entities.User;

namespace EnglishLearningApp.Data.Entities.Document
{
    public class DocumentCategory
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public virtual ICollection<Document> Documents { get; set; }
    }

    public class Document
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string FileUrl { get; set; } = "";
        public Guid CategoryId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual DocumentCategory Category { get; set; }
        public virtual ICollection<UserDocumentHistory> ViewHistories { get; set; }
    }

    public class UserDocumentHistory
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid DocumentId { get; set; }
        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

        public virtual AppUser User { get; set; }
        public virtual Document Document { get; set; }
    }
}
