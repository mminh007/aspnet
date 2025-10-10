using Auth.BLL.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Auth.BLL.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _config;

        public EmailService(ILogger<EmailService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task SendVerificationEmailAsync(string email, string code)
        {
            try
            {
                var smtpHost = _config["SMTP_HOST"];
                var smtpPort = int.Parse(_config["SMTP_PORT"] ?? "25");
                var enableSsl = bool.Parse(_config["SMTP_ENABLE_SSL"] ?? "false");
                var username = _config["SMTP_USERNAME"];
                var password = _config["SMTP_PASSWORD"];
                var fromEmail = _config["SMTP_FROM_EMAIL"] ?? "no-reply@auth.local";
                var fromName = _config["SMTP_FROM_NAME"] ?? "Auth Service";

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = enableSsl,
                    Credentials = string.IsNullOrEmpty(username)
                        ? CredentialCache.DefaultNetworkCredentials
                        : new NetworkCredential(username, password)
                };

                var mail = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = "Xác minh tài khoản",
                    Body = $"Xin chào,\n\nMã xác minh của bạn là: {code}\n\nMã này sẽ hết hạn sau 2 phút.\n\nTrân trọng,\nĐội ngũ Auth Service",
                    IsBodyHtml = false
                };
                mail.To.Add(email);

                await client.SendMailAsync(mail);

                _logger.LogInformation("✅ Verification email sent to {Email} with code {Code}", email, code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to send verification email to {Email}", email);
                throw; // để controller xử lý nếu muốn retry
            }
        }
    }
}
