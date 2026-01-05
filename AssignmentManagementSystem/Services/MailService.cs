using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace AssignmentManagementSystem.Services
{
    public class MailService : IMailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MailService> _logger;
        
        public MailService(IConfiguration configuration, ILogger<MailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var mailSettings = _configuration.GetSection("MailSettings");
                var fromAddress = mailSettings["MailFromAddress"];
                var mailUsername = mailSettings["MailUsername"];
                var mailPassword = mailSettings["MailPassword"];
                var smtpHost = mailSettings["SmtpHost"];
                var smtpPort = int.Parse(mailSettings["SmtpPort"] ?? "587");
                var useSsl = bool.Parse(mailSettings["UseSsl"] ?? "true");
                
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Assignment Management System", fromAddress));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;
                
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = body
                };
                message.Body = bodyBuilder.ToMessageBody();
                
                using var client = new SmtpClient();
                await client.ConnectAsync(smtpHost, smtpPort, useSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                await client.AuthenticateAsync(mailUsername, mailPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                
                _logger.LogInformation($"Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email: {ex.Message}");
                throw;
            }
        }
    }
}
