using EnglishLearningApp.Data;
using EnglishLearningApp.Data.Entities;
using EnglishLearningApp.Data.Entities.Admin;
using EnglishLearningApp.Data.Entities.User;
using EnglishLearningApp.Repository.Interfaces;
using EnglishLearningApp.Service.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EnglishLearningApp.Service.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly AppDbContext _context;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher<AppUser> passwordHasher,
        IConfiguration configuration,
        IEmailService emailService,
        AppDbContext context)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _emailService = emailService;
        _context = context;
    }

    public async Task<object> LoginAsync(object request)
    {
        // Cast request to dynamic to access properties
        dynamic req = request;
        string emailOrPhone = req.EmailOrPhone;
        string password = req.Password;

        // Check if it's email or phone
        var user = emailOrPhone.Contains("@")
            ? await _userRepository.GetByEmailAsync(emailOrPhone)
            : await _userRepository.GetByPhoneAsync(emailOrPhone);        if (user == null)
        {
            throw new UnauthorizedAccessException(emailOrPhone.Contains("@") 
                ? "Email không tồn tại trong hệ thống" 
                : "Số điện thoại không tồn tại trong hệ thống");
        }

        // Check if account is active
        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên để được hỗ trợ.");
        }

        // Check teacher approval status if user is a teacher
        if (user.Role == "Teacher")
        {
            var approval = await _context.TeacherApprovals
                .FirstOrDefaultAsync(ta => ta.UserId == user.Id);

            // If no approval record exists, teacher was created before approval system or somehow bypassed it
            if (approval == null)
            {
                throw new UnauthorizedAccessException("Tài khoản giáo viên chưa được phê duyệt. Vui lòng liên hệ quản trị viên.");
            }

            if (approval.Status == "Pending")
            {
                throw new UnauthorizedAccessException("Tài khoản của bạn đang chờ phê duyệt từ quản trị viên");
            }

            if (approval.Status == "Rejected")
            {
                throw new UnauthorizedAccessException("Tài khoản của bạn đã bị từ chối. Lý do: " + (approval.RejectionReason ?? "Không rõ"));
            }
        }var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Mật khẩu không chính xác");
        }        var token = GenerateJwtToken(user);

        return new
        {
            Token = token,
            User = new
            {
                Id = user.Id.ToString(),
                Name = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            }
        };
    }

    public async Task<object> RegisterAsync(object request)
    {
        dynamic req = request;
        string name = req.Name;
        string email = req.Email;
        string password = req.Password;
        string? phoneNumber = req.PhoneNumber;
        string role = req.Role ?? "Student";

        // Validate password strength
        ValidatePassword(password);

        // Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        if (!string.IsNullOrEmpty(phoneNumber))
        {
            var existingPhoneUser = await _userRepository.GetByPhoneAsync(phoneNumber);
            if (existingPhoneUser != null)
            {
                throw new InvalidOperationException("User with this phone number already exists");
            }
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            FullName = name,
            Email = email,
            PhoneNumber = phoneNumber,
            Role = role,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, password);        var createdUser = await _userRepository.CreateAsync(user);
        
        // If user is a teacher, create a TeacherApproval record with Pending status
        if (role == "Teacher")
        {
            var teacherApproval = new TeacherApproval
            {
                Id = Guid.NewGuid(),
                UserId = createdUser.Id,
                FullName = createdUser.FullName,
                Email = createdUser.Email,
                PhoneNumber = createdUser.PhoneNumber,
                Qualification = "Chưa cập nhật", // Default values
                Experience = "Chưa cập nhật",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.TeacherApprovals.Add(teacherApproval);
            await _context.SaveChangesAsync();            // Return response indicating pending approval (don't generate token)
            return new
            {
                Message = "Đăng ký thành công! Tài khoản giáo viên của bạn đang chờ phê duyệt từ quản trị viên.",
                User = new
                {
                    Id = createdUser.Id.ToString(),
                    Name = createdUser.FullName,
                    Email = createdUser.Email,
                    PhoneNumber = createdUser.PhoneNumber,
                    AvatarUrl = createdUser.AvatarUrl,
                    Role = createdUser.Role,
                    Status = "Pending",
                    CreatedAt = createdUser.CreatedAt
                }
            };
        }
        
        var token = GenerateJwtToken(createdUser);

        return new
        {
            Token = token,
            User = new
            {
                Id = createdUser.Id.ToString(),
                Name = createdUser.FullName,
                Email = createdUser.Email,
                PhoneNumber = createdUser.PhoneNumber,
                AvatarUrl = createdUser.AvatarUrl,
                Role = createdUser.Role,
                CreatedAt = createdUser.CreatedAt
            }
        };
    }

    private void ValidatePassword(string password)
    {
        if (password.Length < 8)
        {
            throw new InvalidOperationException("Mật khẩu phải có ít nhất 8 ký tự");
        }

        if (!password.Any(char.IsUpper))
        {
            throw new InvalidOperationException("Mật khẩu phải chứa ít nhất 1 chữ hoa");
        }

        if (!password.Any(char.IsLower))
        {
            throw new InvalidOperationException("Mật khẩu phải chứa ít nhất 1 chữ thường");
        }

        if (!password.Any(char.IsDigit))
        {
            throw new InvalidOperationException("Mật khẩu phải chứa ít nhất 1 số");
        }

        var specialChars = "@$!%*?&";
        if (!password.Any(c => specialChars.Contains(c)))
        {
            throw new InvalidOperationException("Mật khẩu phải chứa ít nhất 1 ký tự đặc biệt (@$!%*?&)");
        }
    }

    public async Task<object> LoginWithPhoneAsync(object request)
    {
        dynamic req = request;
        string phoneNumber = req.PhoneNumber;
        string verificationCode = req.VerificationCode;
        bool createAccount = req.CreateAccount;
        string? name = req.Name;
        string? email = req.Email;

        // In a real app, verify the code with SMS service
        if (verificationCode.Length != 6)
        {
            throw new UnauthorizedAccessException("Invalid verification code");
        }

        AppUser user;

        if (createAccount)
        {
            var existingUser = await _userRepository.GetByPhoneAsync(phoneNumber);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this phone number already exists");
            }

            user = new AppUser
            {
                Id = Guid.NewGuid(),
                FullName = name ?? "User",
                Email = email ?? "",
                PhoneNumber = phoneNumber,
                PhoneNumberConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PasswordHash = ""
            };

            user = await _userRepository.CreateAsync(user);
        }        else
        {
            user = await _userRepository.GetByPhoneAsync(phoneNumber);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            // Check if account is active
            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên để được hỗ trợ.");
            }
        }        var token = GenerateJwtToken(user);

        return new
        {
            Token = token,
            User = new
            {
                Id = user.Id.ToString(),
                Name = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            }
        };
    }

    public async Task<string> SendVerificationCodeAsync(object request)
    {
        // In a real app, send SMS via Twilio/AWS SNS
        var code = new Random().Next(100000, 999999).ToString();
        return await Task.FromResult(code);
    }

    public async Task<bool> SendPasswordResetCodeAsync(string email)
    {
        // Check if user exists
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            // Return true anyway to prevent email enumeration
            return true;
        }

        // Delete any existing expired tokens
        await _userRepository.DeleteExpiredTokensAsync(email);

        // Generate 6-digit code
        var resetCode = new Random().Next(100000, 999999).ToString();

        // Create reset token
        var token = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Email = email,
            ResetCode = resetCode,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            IsUsed = false
        };

        await _userRepository.CreateResetTokenAsync(token);

        // Send email
        try
        {
            await _emailService.SendPasswordResetCodeAsync(email, resetCode);
        }
        catch (Exception)
        {
            // Log error but don't expose it to user
            // In production, you might want to handle this differently
        }

        return true;
    }

    public async Task<bool> VerifyResetCodeAsync(string email, string code)
    {
        var token = await _userRepository.GetResetTokenAsync(email, code);
        return token != null;
    }

    public async Task<bool> ResetPasswordAsync(string email, string code, string newPassword)
    {
        // Validate password strength
        ValidatePassword(newPassword);

        // Get and validate reset token
        var token = await _userRepository.GetResetTokenAsync(email, code);
        if (token == null)
        {
            throw new InvalidOperationException("Mã xác thực không hợp lệ hoặc đã hết hạn");
        }

        // Get user
        var user = await _userRepository.GetByIdAsync(token.UserId);
        if (user == null)
        {
            throw new InvalidOperationException("Người dùng không tồn tại");
        }

        // Update password
        user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        // Mark token as used
        await _userRepository.MarkTokenAsUsedAsync(token.Id);        return true;
    }

    private string GenerateJwtToken(AppUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? "your-secret-key-here-make-it-long-enough"));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "FPTLearnifyAI",
            audience: _configuration["Jwt:Audience"] ?? "FPTLearnifyAI",
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
