using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EnglishLearningApp.Api.DTOs;
using EnglishLearningApp.Service.Interfaces;
using System.Security.Claims;

namespace EnglishLearningApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClassController : ControllerBase
    {
        private readonly IClassService _classService;

        public ClassController(IClassService classService)
        {
            _classService = classService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyClasses()
        {
            try
            {
                var userId = GetCurrentUserId();
                var classes = await _classService.GetUserClassesAsync(userId);
                return Ok(classes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get classes", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClassDetails(Guid id)
        {
            try
            {
                var classRoom = await _classService.GetByIdAsync(id);
                if (classRoom == null)
                {
                    return NotFound(new { message = "Class not found" });
                }
                return Ok(classRoom);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get class details", error = ex.Message });
            }
        }

        [HttpGet("{id}/members")]
        public async Task<IActionResult> GetClassMembers(Guid id)
        {
            try
            {
                var members = await _classService.GetMembersAsync(id);
                return Ok(members);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get class members", error = ex.Message });
            }
        }        [HttpPost]
        public async Task<IActionResult> CreateClass([FromBody] CreateClassRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                
                // Chỉ cho phép Teacher tạo lớp
                if (userRole != "Teacher")
                {
                    return Forbid();
                }
                
                var classRoom = await _classService.CreateAsync(userId, request.Name, request.Description);
                return Ok(classRoom);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to create class", error = ex.Message });
            }
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinClass([FromBody] JoinClassRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var classRoom = await _classService.JoinClassAsync(userId, request.InviteCode);
                return Ok(classRoom);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to join class", error = ex.Message });
            }
        }

        [HttpDelete("{id}/leave")]
        public async Task<IActionResult> LeaveClass(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _classService.LeaveClassAsync(userId, id);
                if (!success)
                {
                    return NotFound(new { message = "You are not a member of this class" });
                }
                return Ok(new { message = "Left class successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to leave class", error = ex.Message });
            }
        }        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                throw new UnauthorizedAccessException("User not authenticated");

            return Guid.Parse(userIdClaim.Value);
        }

        private string GetCurrentUserRole()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            return roleClaim?.Value ?? "Student";
        }
    }
}
