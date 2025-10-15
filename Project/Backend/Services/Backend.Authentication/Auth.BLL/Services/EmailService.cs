using Auth.BLL.Services.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _config;

    public EmailService(ILogger<EmailService> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    private async Task SendEmailAsync(string to, string subject, string htmlBody, string textBody = "")
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_config["SMTP_FROM_NAME"], _config["SMTP_FROM_EMAIL"]));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Date = DateTimeOffset.UtcNow;
        message.Priority = MessagePriority.Urgent;

        var builder = new BodyBuilder
        {
            HtmlBody = htmlBody,
            TextBody = string.IsNullOrEmpty(textBody)
                ? "Please open this email in an HTML-compatible client."
                : textBody
        };
        message.Body = builder.ToMessageBody();

        try
        {
            using var client = new SmtpClient();
            var host = _config["SMTP_HOST"];
            var port = int.Parse(_config["SMTP_PORT"] ?? "587");
            var enableSsl = bool.Parse(_config["SMTP_ENABLE_SSL"] ?? "true");

            // Timeout để tránh treo
            client.Timeout = 10000;

            await client.ConnectAsync(host, port, enableSsl);
            var username = _config["SMTP_USERNAME"];
            var password = _config["SMTP_PASSWORD"];

            if (!string.IsNullOrEmpty(username))
                await client.AuthenticateAsync(username, password);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("📧 Email sent to {Email} with subject: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send email to {Email}", to);
            throw; // để service biết email chưa gửi được
        }
    }

    public async Task SendVerificationEmailAsync(string email, string code)
    {
        var htmlBody = $@"
        <div style='font-family:Segoe UI, sans-serif; max-width:600px; margin:auto;'>
            <h2 style='color:#4a90e2;'>Xác minh tài khoản của bạn</h2>
            <p>Xin chào,</p>
            <p>Cảm ơn bạn đã đăng ký tài khoản. Mã xác minh của bạn là:</p>
            <div style='background-color:#f5f5f5; padding:10px 20px; font-size:18px; font-weight:bold; text-align:center; border-radius:6px;'>
                {code}
            </div>
            <p>Mã này sẽ hết hạn sau <b>2 phút phút</b>. Nếu bạn không yêu cầu tạo tài khoản, vui lòng bỏ qua email này.</p>
            <p>Trân trọng,<br><b>Auth Service Team</b></p>
        </div>";

        await SendEmailAsync(email, "Xác minh tài khoản", htmlBody);
    }

    public async Task SendPasswordResetEmailAsync(string email, string resetLink, string code)
    {
        var htmlBody = $@"
        <div style='font-family:Segoe UI, sans-serif; max-width:600px; margin:auto;'>
            <h2 style='color:#e24a4a;'>Đặt lại mật khẩu</h2>
            <p>Xin chào,</p>
            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu của bạn.</p>

            <p>Mã xác minh của bạn là:</p>
            <div style='background-color:#f5f5f5; padding:10px 20px; font-size:18px; font-weight:bold; text-align:center; border-radius:6px;'>
                {code}
            </div>

            <p>Nhấp vào liên kết bên dưới để đặt lại mật khẩu (hết hạn sau 5 phút):</p>
            <p style='text-align:center; margin:20px 0;'>
                <a href='{resetLink}' style='background:#4a90e2; color:#fff; padding:12px 24px; text-decoration:none; border-radius:5px;'>
                    Đặt lại mật khẩu
                </a>
            </p>
            <p>Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email này.</p>
            <p>Trân trọng,<br><b>Auth Service Team</b></p>
        </div>";

        await SendEmailAsync(email, "Reset Password", htmlBody);
    }
}
