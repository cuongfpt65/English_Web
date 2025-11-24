namespace EnglishLearningApp.Repository.Interfaces
{
    public interface IAdminRepository
    {
        // Teacher Approval Management
        Task<object> CreateTeacherApprovalAsync(Guid userId, string fullName, string email, string? phoneNumber, string qualification, string experience, string? certificateUrl);
        Task<IEnumerable<object>> GetPendingApprovalsAsync();
        Task<IEnumerable<object>> GetAllApprovalsAsync();
        Task<object?> GetApprovalByIdAsync(Guid id);
        Task<bool> ApproveTeacherAsync(Guid approvalId, Guid adminId);
        Task<bool> RejectTeacherAsync(Guid approvalId, Guid adminId, string reason);
        
        // Statistics
        Task<object> GetDashboardStatisticsAsync();
        Task<object> GetUserStatisticsAsync();
        Task<object> GetClassStatisticsAsync();
        Task<object> GetVocabularyStatisticsAsync();
        Task<object> GetQuizStatisticsAsync();
        Task<object> GetChatStatisticsAsync();
        Task<IEnumerable<object>> GetRecentActivitiesAsync(int limit = 10);
        
        // User Management
        Task<IEnumerable<object>> GetAllUsersAsync();
        Task<bool> ToggleUserStatusAsync(Guid userId, bool isActive);
        Task<bool> ChangeUserRoleAsync(Guid userId, string role);
    }
}
