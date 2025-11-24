using EnglishLearningApp.Data.Entities.Admin;
using EnglishLearningApp.Data.Entities.Chatbot;
using EnglishLearningApp.Data.Entities.Class;
using EnglishLearningApp.Data.Entities.Document;
using EnglishLearningApp.Data.Entities.Game;
using EnglishLearningApp.Data.Entities.Test;
using EnglishLearningApp.Data.Entities.User;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace EnglishLearningApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }        // --- User ---
        public DbSet<AppUser> Users { get; set; }
        public DbSet<UserGoogleLogin> UserGoogleLogins { get; set; }
        public DbSet<PhoneVerification> PhoneVerifications { get; set; }

        // --- Chatbot ---
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Vocabulary> Vocabularies { get; set; }
        public DbSet<UserVocabulary> UserVocabularies { get; set; }
        public DbSet<GrammarNote> GrammarNotes { get; set; }
        public DbSet<TranslationHistory> TranslationHistories { get; set; }

        // --- Game ---
        public DbSet<Game> Games { get; set; }
        public DbSet<GameQuestion> GameQuestions { get; set; }
        public DbSet<UserGameScore> UserGameScores { get; set; }        // --- Class ---
        public DbSet<ClassRoom> ClassRooms { get; set; }
        public DbSet<ClassMember> ClassMembers { get; set; }
        public DbSet<ClassMessage> ClassMessages { get; set; }
        public DbSet<ClassQuiz> ClassQuizzes { get; set; }
        public DbSet<ClassQuizQuestion> ClassQuizQuestions { get; set; }
        public DbSet<ClassQuizAttempt> ClassQuizAttempts { get; set; }
        public DbSet<ClassQuizAnswer> ClassQuizAnswers { get; set; }
        public DbSet<ClassMemberStats> ClassMemberStats { get; set; }
        public DbSet<ClassAchievement> ClassAchievements { get; set; }

        // --- Test ---
        public DbSet<Test> Tests { get; set; }
        public DbSet<TestQuestion> TestQuestions { get; set; }
        public DbSet<TestResult> TestResults { get; set; }
        public DbSet<UserTestAnswer> UserTestAnswers { get; set; }        // --- Document ---
        public DbSet<DocumentCategory> DocumentCategories { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<UserDocumentHistory> UserDocumentHistories { get; set; }

        // --- Admin ---
        public DbSet<TeacherApproval> TeacherApprovals { get; set; }
        public DbSet<SystemStatistics> SystemStatistics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình quan hệ quan trọng

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }


        }
    }
}
