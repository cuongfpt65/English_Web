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

        public Task SendEmailVerificationCodeAsync(string email, string code)
        {
            // Log to console instead of sending real email
            var message = $@"
╔══════════════════════════════════════════════════════════════╗
║           📧 MOCK EMAIL SERVICE (Development Mode)          ║
╚══════════════════════════════════════════════════════════════╝

TO: {email}
SUBJECT: Xác thực email đăng ký tài khoản - FPT Learnify AI

═══════════════════════════════════════════════════════════════

Chào mừng bạn đến với FPT Learnify AI!

Để hoàn tất quá trình đăng ký, vui lòng sử dụng mã xác thực bên dưới:

MÃ XÁC THỰC CỦA BẠN:
╔═══════════╗
║  {code}  ║
╚═══════════╝

⏰ Mã này có hiệu lực trong 15 phút.

📝 LƯU Ý:
• Không chia sẻ mã này với bất kỳ ai
• Mã sẽ hết hạn sau 15 phút
• Nếu bạn không thực hiện đăng ký này, vui lòng bỏ qua email này

Sau khi xác thực thành công, bạn có thể bắt đầu hành trình học tiếng Anh
thú vị với chúng tôi! 🚀

═══════════════════════════════════════════════════════════════

© 2024 FPT Learnify AI
";

            _logger.LogInformation(message);
            Console.WriteLine(message);

            return Task.CompletedTask;
        }
    }
}
