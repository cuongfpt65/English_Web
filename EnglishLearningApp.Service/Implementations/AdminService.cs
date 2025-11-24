using EnglishLearningApp.Repository.Interfaces;
using EnglishLearningApp.Service.Interfaces;

namespace EnglishLearningApp.Service.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepository;

        public AdminService(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public async Task<object> RequestTeacherApprovalAsync(Guid userId, string fullName, string email, string? phoneNumber, string qualification, string experience, string? certificateUrl)
        {
            return await _adminRepository.CreateTeacherApprovalAsync(userId, fullName, email, phoneNumber, qualification, experience, certificateUrl);
        }

        public async Task<IEnumerable<object>> GetPendingApprovalsAsync()
        {
            return await _adminRepository.GetPendingApprovalsAsync();
        }

        public async Task<IEnumerable<object>> GetAllApprovalsAsync()
        {
            return await _adminRepository.GetAllApprovalsAsync();
        }

        public async Task<object?> GetApprovalByIdAsync(Guid id)
        {
            return await _adminRepository.GetApprovalByIdAsync(id);
        }

        public async Task<bool> ApproveTeacherAsync(Guid approvalId, Guid adminId)
        {
            return await _adminRepository.ApproveTeacherAsync(approvalId, adminId);
        }

        public async Task<bool> RejectTeacherAsync(Guid approvalId, Guid adminId, string reason)
        {
            return await _adminRepository.RejectTeacherAsync(approvalId, adminId, reason);
        }

        public async Task<object> GetDashboardStatisticsAsync()
        {
            return await _adminRepository.GetDashboardStatisticsAsync();
        }

        public async Task<object> GetUserStatisticsAsync()
        {
            return await _adminRepository.GetUserStatisticsAsync();
        }

        public async Task<object> GetClassStatisticsAsync()
        {
            return await _adminRepository.GetClassStatisticsAsync();
        }

        public async Task<object> GetVocabularyStatisticsAsync()
        {
            return await _adminRepository.GetVocabularyStatisticsAsync();
        }

        public async Task<object> GetQuizStatisticsAsync()
        {
            return await _adminRepository.GetQuizStatisticsAsync();
        }

        public async Task<object> GetChatStatisticsAsync()
        {
            return await _adminRepository.GetChatStatisticsAsync();
        }

        public async Task<IEnumerable<object>> GetRecentActivitiesAsync(int limit = 10)
        {
            return await _adminRepository.GetRecentActivitiesAsync(limit);
        }

        public async Task<IEnumerable<object>> GetAllUsersAsync()
        {
            return await _adminRepository.GetAllUsersAsync();
        }

        public async Task<bool> ToggleUserStatusAsync(Guid userId, bool isActive)
        {
            return await _adminRepository.ToggleUserStatusAsync(userId, isActive);
        }

        public async Task<bool> ChangeUserRoleAsync(Guid userId, string role)
        {
            return await _adminRepository.ChangeUserRoleAsync(userId, role);
        }
    }
}
