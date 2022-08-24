using AspNetIdentityDemo.Api.Services.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace AspNetIdentityDemo.Api.Services.Implementation_Classes
{
    public class MailService: IMailService
    {
        private IConfiguration _configuration;

        public MailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendVerificationEmailAsync(string toEmail, string subject, string content)
        {
            var apiKey = _configuration["SendGrid:APIKey"];
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_configuration["SendGrid:FromEmail"], "JWT Auth Demo");
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, content, content);
            var response = await client.SendEmailAsync(msg);
        }
    }
}
