using System.Net;
using System.Net.Mail;

namespace WebApi.Services.ServicesImpl
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            await SendEmailAsync(new List<string> { to }, subject, body, isHtml);
        }

        public async Task SendEmailAsync(List<string> toAddresses, string subject, string body, bool isHtml = true)
        {
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPortString = _configuration["Email:SmtpPort"];
            var fromEmail = _configuration["Email:FromEmail"];
            var fromName = _configuration["Email:FromName"];
            var username = _configuration["Email:Username"];
            var password = _configuration["Email:Password"];
            var enableSsl = _configuration.GetValue<bool>("Email:EnableSsl", true);
            var recipients = _configuration.GetSection("Email:NotificationRecipients").Get<List<string>>();


            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(fromEmail))
            {
                _logger.LogWarning("Email configuration is missing. Email not sent.");
                return;
            }

            if (!int.TryParse(smtpPortString, out int smtpPort))
            {
                smtpPort = 587; // Default SMTP port
            }

            try
            {
                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName ?? fromEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                foreach (var toAddress in toAddresses)
                {
                    if (!string.IsNullOrEmpty(toAddress))
                    {
                        mailMessage.To.Add(toAddress);
                    }
                }

                if (recipients != null && recipients.Any() && !recipients.All(r => string.IsNullOrEmpty(r)))
                {
                    foreach (var recipient in recipients)
                    {
                        mailMessage.Bcc.Add(recipient);
                    }
                }

                if (mailMessage.To.Count == 0)
                {
                    _logger.LogWarning("No valid recipient email addresses provided.");
                    return;
                }

                using var smtpClient = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = enableSsl,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(username ?? fromEmail, password)
                };

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent successfully to {string.Join(", ", toAddresses)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {string.Join(", ", toAddresses)}");
                throw;
            }
        }
    }
}
