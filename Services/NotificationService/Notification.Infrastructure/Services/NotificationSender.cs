using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using Notification.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Notification.Domain.Settings;

namespace Notification.Infrastructure.Services;

public class NotificationSender : INotificationSender
{
    private readonly ILogger<NotificationSender> _logger;
    private readonly NotificationOptions _opts;

    public NotificationSender(IOptions<NotificationOptions> opts, ILogger<NotificationSender> logger)
    {
        _logger = logger;
        _opts = opts.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var msg = new MimeMessage();
        msg.From.Add(MailboxAddress.Parse(_opts.Smtp.From));
        msg.To.Add(MailboxAddress.Parse(to));
        msg.Subject = subject;
        msg.Body = new TextPart("plain") { Text = body };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_opts.Smtp.Host, _opts.Smtp.Port, MailKit.Security.SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_opts.Smtp.Username, _opts.Smtp.Password);
        await smtp.SendAsync(msg);
        await smtp.DisconnectAsync(true);
        _logger.LogInformation("📧 Email sent to {To}", to);
    }

    public Task SendSmsAsync(string phoneNumber, string message)
    {
        _logger.LogWarning("📱 SMS/WhatsApp not configured - Message to {Phone} not sent: {Message}", phoneNumber, message);
        return Task.CompletedTask;
    }

    public Task SendPushAsync(string userId, string title, string message)
    {
        _logger.LogInformation("🔔 Push not implemented (user {User})", userId);
        return Task.CompletedTask;
    }
}