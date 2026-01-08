using EnglishLearningApp.Data.Entities.User;

namespace EnglishLearningApp.Data.Entities.Document
{
    public class DocumentCategory
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    }

    public class Document
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string FileName { get; set; } = "";
        public string FileUrl { get; set; } = "";
        public string FileType { get; set; } = ""; // pdf, docx, doc, etc
        public long FileSize { get; set; } = 0; // in bytes
        public Guid CategoryId { get; set; }
        public Guid UploadedByUserId { get; set; }
        public int ViewCount { get; set; } = 0;
        public int DownloadCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual DocumentCategory Category { get; set; } = null!;
        public virtual AppUser UploadedBy { get; set; } = null!;
        public virtual ICollection<UserDocumentHistory> ViewHistories { get; set; } = new List<UserDocumentHistory>();
    }

    public class UserDocumentHistory
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid DocumentId { get; set; }
        public string Action { get; set; } = "View"; // View, Download
        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

        public virtual AppUser User { get; set; } = null!;
        public virtual Document Document { get; set; } = null!;
    }
}
