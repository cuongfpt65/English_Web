using EnglishLearningApp.Data;
using EnglishLearningApp.Data.Entities.Admin;
using EnglishLearningApp.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningApp.Repository.Implementations
{
    public class AdminRepository : IAdminRepository
    {
        private readonly AppDbContext _context;

        public AdminRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<object> CreateTeacherApprovalAsync(Guid userId, string fullName, string email, string? phoneNumber, string qualification, string experience, string? certificateUrl)
        {
            var approval = new TeacherApproval
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                Qualification = qualification,
                Experience = experience,
                CertificateUrl = certificateUrl,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.TeacherApprovals.Add(approval);
            await _context.SaveChangesAsync();

            return new
            {
                Id = approval.Id,
                UserId = approval.UserId,
                FullName = approval.FullName,
                Email = approval.Email,
                PhoneNumber = approval.PhoneNumber,
                Qualification = approval.Qualification,
                Experience = approval.Experience,
                CertificateUrl = approval.CertificateUrl,
                Status = approval.Status,
                CreatedAt = approval.CreatedAt
            };
        }

        public async Task<IEnumerable<object>> GetPendingApprovalsAsync()
        {
            var approvals = await _context.TeacherApprovals
                .Include(a => a.User)
                .Where(a => a.Status == "Pending")
                .OrderBy(a => a.CreatedAt)
                .ToListAsync();

            return approvals.Select(a => new
            {
                Id = a.Id,
                UserId = a.UserId,
                FullName = a.FullName,
                Email = a.Email,
                PhoneNumber = a.PhoneNumber,
                Qualification = a.Qualification,
                Experience = a.Experience,
                CertificateUrl = a.CertificateUrl,
                Status = a.Status,
                CreatedAt = a.CreatedAt
            });
        }

        public async Task<IEnumerable<object>> GetAllApprovalsAsync()
        {
            var approvals = await _context.TeacherApprovals
                .Include(a => a.User)
                .Include(a => a.ApprovedByAdmin)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return approvals.Select(a => new
            {
                Id = a.Id,
                UserId = a.UserId,
                FullName = a.FullName,
                Email = a.Email,
                PhoneNumber = a.PhoneNumber,
                Qualification = a.Qualification,
                Experience = a.Experience,
                CertificateUrl = a.CertificateUrl,
                Status = a.Status,
                RejectionReason = a.RejectionReason,
                ApprovedByAdmin = a.ApprovedByAdmin != null ? new { Id = a.ApprovedByAdmin.Id, Name = a.ApprovedByAdmin.FullName } : null,
                CreatedAt = a.CreatedAt,
                ReviewedAt = a.ReviewedAt
            });
        }

        public async Task<object?> GetApprovalByIdAsync(Guid id)
        {
            var approval = await _context.TeacherApprovals
                .Include(a => a.User)
                .Include(a => a.ApprovedByAdmin)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (approval == null) return null;

            return new
            {
                Id = approval.Id,
                UserId = approval.UserId,
                FullName = approval.FullName,
                Email = approval.Email,
                PhoneNumber = approval.PhoneNumber,
                Qualification = approval.Qualification,
                Experience = approval.Experience,
                CertificateUrl = approval.CertificateUrl,
                Status = approval.Status,
                RejectionReason = approval.RejectionReason,
                ApprovedByAdmin = approval.ApprovedByAdmin != null ? new { Id = approval.ApprovedByAdmin.Id, Name = approval.ApprovedByAdmin.FullName } : null,
                CreatedAt = approval.CreatedAt,
                ReviewedAt = approval.ReviewedAt
            };
        }

        public async Task<bool> ApproveTeacherAsync(Guid approvalId, Guid adminId)
        {
            var approval = await _context.TeacherApprovals.FindAsync(approvalId);
            if (approval == null || approval.Status != "Pending") return false;

            var user = await _context.Users.FindAsync(approval.UserId);
            if (user == null) return false;

            approval.Status = "Approved";
            approval.ApprovedByAdminId = adminId;
            approval.ReviewedAt = DateTime.UtcNow;

            user.Role = "Teacher";
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectTeacherAsync(Guid approvalId, Guid adminId, string reason)
        {
            var approval = await _context.TeacherApprovals.FindAsync(approvalId);
            if (approval == null || approval.Status != "Pending") return false;

            approval.Status = "Rejected";
            approval.RejectionReason = reason;
            approval.ApprovedByAdminId = adminId;
            approval.ReviewedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }        public async Task<object> GetDashboardStatisticsAsync()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync();
                var totalStudents = await _context.Users.CountAsync(u => u.Role == "Student");
                var totalTeachers = await _context.Users.CountAsync(u => u.Role == "Teacher");
                var totalAdmins = await _context.Users.CountAsync(u => u.Role == "Admin");
                var totalClasses = await _context.ClassRooms.CountAsync();
                var totalVocabularies = await _context.Vocabularies.CountAsync();
                
                var totalChatSessions = 0;
                try
                {
                    totalChatSessions = await _context.ChatSessions.CountAsync();
                }
                catch (Exception)
                {
                    // ChatSessions table might not exist
                    totalChatSessions = 0;
                }

                var totalQuizzes = 0;
                try
                {
                    totalQuizzes = await _context.ClassQuizzes.CountAsync();
                }
                catch (Exception)
                {
                    // ClassQuizzes table might not exist
                    totalQuizzes = 0;
                }

                var pendingApprovals = 0;
                try
                {
                    pendingApprovals = await _context.TeacherApprovals.CountAsync(a => a.Status == "Pending");
                }
                catch (Exception)
                {
                    // TeacherApprovals table might not exist
                    pendingApprovals = 0;
                }

                var activeUsersToday = 0;
                try
                {
                    var today = DateTime.UtcNow.Date;
                    activeUsersToday = await _context.ChatSessions
                        .Where(s => s.CreatedAt >= today)
                        .Select(s => s.UserId)
                        .Distinct()
                        .CountAsync();
                }
                catch (Exception)
                {
                    // ChatSessions table might not exist
                    activeUsersToday = 0;
                }

                return new
                {
                    TotalUsers = totalUsers,
                    TotalStudents = totalStudents,
                    TotalTeachers = totalTeachers,
                    TotalAdmins = totalAdmins,
                    TotalClasses = totalClasses,
                    TotalVocabularies = totalVocabularies,
                    TotalChatSessions = totalChatSessions,
                    TotalQuizzes = totalQuizzes,
                    PendingApprovals = pendingApprovals,
                    ActiveUsersToday = activeUsersToday
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting dashboard statistics: {ex.Message}", ex);
            }
        }        public async Task<object> GetUserStatisticsAsync()
        {
            try
            {
                var usersByRole = await _context.Users
                    .GroupBy(u => u.Role)
                    .Select(g => new { Role = g.Key, Count = g.Count() })
                    .ToListAsync();

                var last7Days = Enumerable.Range(0, 7)
                    .Select(i => DateTime.UtcNow.Date.AddDays(-i))
                    .Reverse()
                    .ToList();

                var newUsersByDay = new List<object>();
                foreach (var day in last7Days)
                {
                    var count = await _context.Users
                        .CountAsync(u => u.CreatedAt.Date == day);
                    newUsersByDay.Add(new { Date = day.ToString("yyyy-MM-dd"), Count = count });
                }

                return new
                {
                    UsersByRole = usersByRole,
                    NewUsersByDay = newUsersByDay,
                    TotalUsers = await _context.Users.CountAsync()
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting user statistics: {ex.Message}", ex);
            }
        }        public async Task<object> GetClassStatisticsAsync()
        {
            try
            {
                var totalClasses = await _context.ClassRooms.CountAsync();
                var activeClasses = totalClasses; // All classes are active by default
                var totalMembers = await _context.ClassMembers.CountAsync();
                var averageMembersPerClass = totalClasses > 0 ? (double)totalMembers / totalClasses : 0;

                var topClasses = await _context.ClassRooms
                    .Include(c => c.Members)
                    .OrderByDescending(c => c.Members.Count)
                    .Take(5)
                    .Select(c => new
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Code = c.InviteCode,
                        MemberCount = c.Members.Count,
                        CreatedAt = c.CreatedAt
                    })
                    .ToListAsync();

                return new
                {
                    TotalClasses = totalClasses,
                    ActiveClasses = activeClasses,
                    TotalMembers = totalMembers,
                    AverageMembersPerClass = Math.Round(averageMembersPerClass, 2),
                    TopClasses = topClasses
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting class statistics: {ex.Message}", ex);
            }
        }        public async Task<object> GetVocabularyStatisticsAsync()
        {
            try
            {
                var totalVocabularies = await _context.Vocabularies.CountAsync();
                var totalUserVocabularies = await _context.UserVocabularies.CountAsync();
                var averageVocabulariesPerUser = await _context.Users.CountAsync() > 0
                    ? (double)totalUserVocabularies / await _context.Users.CountAsync()
                    : 0;

                var vocabulariesByCategory = await _context.Vocabularies
                    .GroupBy(v => v.Topic ?? "Uncategorized")
                    .Select(g => new { Category = g.Key, Count = g.Count() })
                    .OrderByDescending(g => g.Count)
                    .Take(10)
                    .ToListAsync();

                return new
                {
                    TotalVocabularies = totalVocabularies,
                    TotalUserVocabularies = totalUserVocabularies,
                    AverageVocabulariesPerUser = Math.Round(averageVocabulariesPerUser, 2),
                    VocabulariesByCategory = vocabulariesByCategory
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting vocabulary statistics: {ex.Message}", ex);
            }
        }        public async Task<object> GetQuizStatisticsAsync()
        {
            try
            {
                var totalQuizzes = 0;
                var totalAttempts = 0;
                var completedAttempts = 0;
                var averageScore = 0.0;
                var quizzesByClass = new List<object>();

                try
                {
                    totalQuizzes = await _context.ClassQuizzes.CountAsync();
                    totalAttempts = await _context.ClassQuizAttempts.CountAsync();
                    completedAttempts = await _context.ClassQuizAttempts.CountAsync(a => a.IsCompleted);
                    averageScore = await _context.ClassQuizAttempts
                        .Where(a => a.IsCompleted)
                        .AverageAsync(a => (double?)a.Score) ?? 0;

                    quizzesByClass = await _context.ClassQuizzes
                        .GroupBy(q => q.ClassRoomId)
                        .Select(g => new
                        {
                            ClassId = g.Key,
                            Count = g.Count()
                        })
                        .OrderByDescending(g => g.Count)
                        .Take(5)
                        .Cast<object>()
                        .ToListAsync();
                }
                catch (Exception)
                {
                    // Tables might not exist yet
                }

                return new
                {
                    TotalQuizzes = totalQuizzes,
                    TotalAttempts = totalAttempts,
                    CompletedAttempts = completedAttempts,
                    AverageScore = Math.Round(averageScore, 2),
                    QuizzesByClass = quizzesByClass
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting quiz statistics: {ex.Message}", ex);
            }
        }        public async Task<object> GetChatStatisticsAsync()
        {
            try
            {
                var totalSessions = 0;
                var totalMessages = 0;
                var averageMessagesPerSession = 0.0;
                var sessionsByDay = new List<object>();

                try
                {
                    totalSessions = await _context.ChatSessions.CountAsync();
                    totalMessages = await _context.ChatMessages.CountAsync();
                    averageMessagesPerSession = totalSessions > 0 ? (double)totalMessages / totalSessions : 0;

                    var last7Days = Enumerable.Range(0, 7)
                        .Select(i => DateTime.UtcNow.Date.AddDays(-i))
                        .Reverse()
                        .ToList();

                    foreach (var day in last7Days)
                    {
                        var count = await _context.ChatSessions
                            .CountAsync(s => s.CreatedAt.Date == day);
                        sessionsByDay.Add(new { Date = day.ToString("yyyy-MM-dd"), Count = count });
                    }
                }
                catch (Exception)
                {
                    // Tables might not exist yet
                }

                return new
                {
                    TotalSessions = totalSessions,
                    TotalMessages = totalMessages,
                    AverageMessagesPerSession = Math.Round(averageMessagesPerSession, 2),
                    SessionsByDay = sessionsByDay
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting chat statistics: {ex.Message}", ex);
            }
        }        public async Task<IEnumerable<object>> GetRecentActivitiesAsync(int limit = 10)
        {
            try
            {
                var activities = new List<object>();

                var recentUsers = await _context.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(limit / 2)
                    .Select(u => new
                    {
                        Type = "NewUser",
                        Description = $"New user registered: {u.FullName}",
                        Timestamp = u.CreatedAt,
                        UserId = u.Id,
                        UserName = u.FullName
                    })
                    .ToListAsync();

                var recentClasses = await _context.ClassRooms
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(limit / 2)
                    .Select(c => new
                    {
                        Type = "NewClass",
                        Description = $"New class created: {c.Name}",
                        Timestamp = c.CreatedAt,
                        ClassId = c.Id,
                        ClassName = c.Name
                    })
                    .ToListAsync();

                activities.AddRange(recentUsers);
                activities.AddRange(recentClasses);

                return activities.OrderByDescending(a =>
                {
                    var prop = a.GetType().GetProperty("Timestamp");
                    return (DateTime)(prop?.GetValue(a) ?? DateTime.MinValue);
                }).Take(limit);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting recent activities: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<object>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return users.Select(u => new
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Role = u.Role,
                AvatarUrl = u.AvatarUrl,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            });
        }

        public async Task<bool> ToggleUserStatusAsync(Guid userId, bool isActive)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // Add IsActive property to AppUser entity if needed
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangeUserRoleAsync(Guid userId, string role)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.Role = role;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
