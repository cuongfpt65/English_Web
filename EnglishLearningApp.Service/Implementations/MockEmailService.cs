using EnglishLearningApp.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace EnglishLearningApp.Service.Implementations
{
    /// <summary>
    /// Mock Email Service for development/testing
    /// This service logs the email to console instead of actually sending it
    /// Use this when you don't have SMTP credentials configured
    /// </summary>
    public class MockEmailService : IEmailService
    {
        private readonly ILogger<MockEmailService> _logger;

        public MockEmailService(ILogger<MockEmailService> logger)
        {
            _logger = logger;
        }

        public Task SendPasswordResetCodeAsync(string email, string code)
        {
            // Log to console instead of sending real email
            var message = $@"
╔══════════════════════════════════════════════════════════════╗
║           📧 MOCK EMAIL SERVICE (Development Mode)          ║
╚══════════════════════════════════════════════════════════════╝

TO: {email}
SUBJECT: Mã xác thực đặt lại mật khẩu - FPT Learnify AI

═══════════════════════════════════════════════════════════════

Xin chào,

Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản FPT Learnify AI.

MÃ XÁC THỰC CỦA BẠN:
╔═══════════╗
║  {code}  ║
╚═══════════╝

⏰ Mã này có hiệu lực trong 15 phút.

⚠️  LƯU Ý:
• Không chia sẻ mã này với bất kỳ ai
• Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này

═══════════════════════════════════════════════════════════════

© 2024 FPT Learnify AI
";

            _logger.LogInformation(message);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();

            return Task.CompletedTask;
        }
    }
}
