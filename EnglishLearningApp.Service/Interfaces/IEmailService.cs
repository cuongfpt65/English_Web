namespace EnglishLearningApp.Service.Interfaces
{
    public interface IEmailService
    {
        Task SendPasswordResetCodeAsync(string email, string code);
    }
}
