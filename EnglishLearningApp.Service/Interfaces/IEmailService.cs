namespace EnglishLearningApp.Service.Interfaces
{
    public interface IEmailService
    {
        Task SendPasswordResetCodeAsync(string email, string code);
        Task SendEmailVerificationCodeAsync(string email, string code);
    }
}
