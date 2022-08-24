namespace AspNetIdentityDemo.Api.Services.Interfaces
{
    public interface IMailService
    {
        Task SendVerificationEmailAsync(string toEmail, string subject, string content);
    }
}
