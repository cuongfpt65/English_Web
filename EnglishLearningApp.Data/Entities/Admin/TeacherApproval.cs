namespace EnglishLearningApp.Data.Entities.Admin
{
    public class TeacherApproval
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string Qualification { get; set; } = null!; // Bằng cấp
        public string Experience { get; set; } = null!; // Kinh nghiệm
        public string? CertificateUrl { get; set; } // Link chứng chỉ
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        public string? RejectionReason { get; set; }
        public Guid? ApprovedByAdminId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
        
        public virtual User.AppUser User { get; set; } = null!;
        public virtual User.AppUser? ApprovedByAdmin { get; set; }
    }

    public class SystemStatistics
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalClasses { get; set; }
        public int TotalVocabularies { get; set; }
        public int TotalChatSessions { get; set; }
        public int TotalQuizzes { get; set; }
        public int ActiveUsersToday { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
