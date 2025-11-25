using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnglishLearningApp.Api.DTOs;
using EnglishLearningApp.Service.Interfaces;
using System.Security.Claims;

namespace EnglishLearningApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // Teacher Approval Management

        [HttpPost("teacher-approval")]
        public async Task<IActionResult> RequestTeacherApproval([FromBody] TeacherApprovalRequestDto request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
                var result = await _adminService.RequestTeacherApprovalAsync(
                    userId,
                    request.FullName,
                    request.Email,
                    request.PhoneNumber,
                    request.Qualification,
                    request.Experience,
                    request.CertificateUrl
                );
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to submit teacher approval request", error = ex.Message });
            }
        }

        [HttpGet("teacher-approvals/pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingApprovals()
        {
            try
            {
                var approvals = await _adminService.GetPendingApprovalsAsync();
                return Ok(approvals);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get pending approvals", error = ex.Message });
            }
        }

        [HttpGet("teacher-approvals")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllApprovals()
        {
            try
            {
                var approvals = await _adminService.GetAllApprovalsAsync();
                return Ok(approvals);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get approvals", error = ex.Message });
            }
        }

        [HttpGet("teacher-approvals/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetApprovalById(Guid id)
        {
            try
            {
                var approval = await _adminService.GetApprovalByIdAsync(id);
                if (approval == null)
                {
                    return NotFound(new { message = "Approval not found" });
                }
                return Ok(approval);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get approval", error = ex.Message });
            }
        }

        [HttpPost("teacher-approvals/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveTeacher([FromBody] ApprovalActionDto request)
        {
            try
            {
                var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
                var result = await _adminService.ApproveTeacherAsync(request.ApprovalId, adminId);
                if (!result)
                {
                    return BadRequest(new { message = "Failed to approve teacher" });
                }
                return Ok(new { message = "Teacher approved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to approve teacher", error = ex.Message });
            }
        }

        [HttpPost("teacher-approvals/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectTeacher([FromBody] ApprovalActionDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RejectionReason))
                {
                    return BadRequest(new { message = "Rejection reason is required" });
                }

                var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
                var result = await _adminService.RejectTeacherAsync(request.ApprovalId, adminId, request.RejectionReason);
                if (!result)
                {
                    return BadRequest(new { message = "Failed to reject teacher" });
                }
                return Ok(new { message = "Teacher approval rejected" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to reject teacher", error = ex.Message });
            }
        }

        [HttpPost("users/{userId}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveTeacher(Guid userId)
        {
            try
            {
                await _adminService.ApproveTeacherAsync(userId);
                return Ok(new { message = "Teacher approved successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to approve teacher", error = ex.Message });
            }
        }

        [HttpPost("users/{userId}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectTeacher(Guid userId, [FromBody] RejectTeacherDto request)
        {
            try
            {
                await _adminService.RejectTeacherAsync(userId, request.Reason);
                return Ok(new { message = "Teacher rejected successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to reject teacher", error = ex.Message });
            }
        }

        [HttpGet("users/pending-teachers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingTeachers()
        {
            try
            {
                var teachers = await _adminService.GetPendingTeachersAsync();
                return Ok(teachers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get pending teachers", error = ex.Message });
            }
        }

        // Statistics

        [HttpGet("statistics/dashboard")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDashboardStatistics()
        {
            try
            {
                var stats = await _adminService.GetDashboardStatisticsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get dashboard statistics", error = ex.Message });
            }
        }

        [HttpGet("statistics/users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserStatistics()
        {
            try
            {
                var stats = await _adminService.GetUserStatisticsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get user statistics", error = ex.Message });
            }
        }

        [HttpGet("statistics/classes")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetClassStatistics()
        {
            try
            {
                var stats = await _adminService.GetClassStatisticsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get class statistics", error = ex.Message });
            }
        }

        [HttpGet("statistics/vocabularies")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetVocabularyStatistics()
        {
            try
            {
                var stats = await _adminService.GetVocabularyStatisticsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get vocabulary statistics", error = ex.Message });
            }
        }

        [HttpGet("statistics/quizzes")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetQuizStatistics()
        {
            try
            {
                var stats = await _adminService.GetQuizStatisticsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get quiz statistics", error = ex.Message });
            }
        }

        [HttpGet("statistics/chat")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetChatStatistics()
        {
            try
            {
                var stats = await _adminService.GetChatStatisticsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get chat statistics", error = ex.Message });
            }
        }

        [HttpGet("activities/recent")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRecentActivities([FromQuery] int limit = 10)
        {
            try
            {
                var activities = await _adminService.GetRecentActivitiesAsync(limit);
                return Ok(activities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get recent activities", error = ex.Message });
            }
        }

        // User Management

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _adminService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get users", error = ex.Message });
            }
        }

        [HttpPut("users/role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeUserRole([FromBody] ChangeUserRoleDto request)
        {
            try
            {
                var result = await _adminService.ChangeUserRoleAsync(request.UserId, request.Role);
                if (!result)
                {
                    return BadRequest(new { message = "Failed to change user role" });
                }
                return Ok(new { message = "User role changed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to change user role", error = ex.Message });
            }
        }

        [HttpPut("users/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleUserStatus([FromBody] ToggleUserStatusDto request)
        {
            try
            {
                var result = await _adminService.ToggleUserStatusAsync(request.UserId, request.IsActive);
                if (!result)
                {
                    return BadRequest(new { message = "Failed to toggle user status" });
                }
                return Ok(new { message = "User status updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to toggle user status", error = ex.Message });
            }
        }
    }
}
