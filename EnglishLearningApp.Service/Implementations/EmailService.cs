using EnglishLearningApp.Service.Interfaces;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace EnglishLearningApp.Service.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendPasswordResetCodeAsync(string email, string code)
        {
            try
            {
                var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["Email:Username"] ?? "";
                var smtpPassword = _configuration["Email:Password"] ?? "";
                var fromEmail = _configuration["Email:From"] ?? smtpUsername;
                var fromName = _configuration["Email:FromName"] ?? "FPT Learnify AI";

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = "Mã xác thực đặt lại mật khẩu - FPT Learnify AI",
                    Body = GetEmailBody(code),
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // Log the error (in production, use proper logging)
                Console.WriteLine($"Failed to send email: {ex.Message}");
                // In development, we'll just continue without throwing
                // In production, you might want to throw or handle differently
            }
        }

        private string GetEmailBody(string code)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 600px;
            margin: 40px auto;
            background-color: #ffffff;
            border-radius: 10px;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
            overflow: hidden;
        }}
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 28px;
        }}
        .content {{
            padding: 40px 30px;
        }}
        .code-box {{
            background-color: #f8f9fa;
            border: 2px dashed #667eea;
            border-radius: 8px;
            padding: 20px;
            text-align: center;
            margin: 30px 0;
        }}
        .code {{
            font-size: 36px;
            font-weight: bold;
            color: #667eea;
            letter-spacing: 8px;
            font-family: 'Courier New', monospace;
        }}
        .warning {{
            background-color: #fff3cd;
            border-left: 4px solid #ffc107;
            padding: 15px;
            margin: 20px 0;
            color: #856404;
        }}
        .footer {{
            background-color: #f8f9fa;
            padding: 20px;
            text-align: center;
            color: #6c757d;
            font-size: 14px;
        }}
        .button {{
            display: inline-block;
            padding: 12px 30px;
            background-color: #667eea;
            color: white;
            text-decoration: none;
            border-radius: 5px;
            margin-top: 20px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔐 Đặt lại mật khẩu</h1>
        </div>
        <div class='content'>
            <p>Xin chào,</p>
            <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản FPT Learnify AI của mình. Sử dụng mã xác thực bên dưới để hoàn tất quá trình:</p>
            
            <div class='code-box'>
                <p style='margin: 0; color: #6c757d; font-size: 14px;'>MÃ XÁC THỰC CỦA BẠN</p>
                <p class='code'>{code}</p>
                <p style='margin: 0; color: #6c757d; font-size: 12px;'>Mã có hiệu lực trong 15 phút</p>
            </div>

            <div class='warning'>
                <strong>⚠️ Lưu ý:</strong>
                <ul style='margin: 10px 0 0 0; padding-left: 20px;'>
                    <li>Không chia sẻ mã này với bất kỳ ai</li>
                    <li>Mã sẽ hết hạn sau 15 phút</li>
                    <li>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này</li>
                </ul>
            </div>

            <p style='color: #6c757d; font-size: 14px; margin-top: 30px;'>
                Nếu bạn gặp vấn đề, vui lòng liên hệ với chúng tôi qua email hỗ trợ.
            </p>
        </div>
        <div class='footer'>
            <p>© 2024 FPT Learnify AI - Nền tảng học tiếng Anh thông minh</p>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
