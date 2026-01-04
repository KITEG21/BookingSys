using Microsoft.Extensions.Logging;
using Notification.Application.Interfaces;

namespace Notification.Infrastructure.Services;

/// <summary>
/// Simulated notification sender - logs messages instead of actually sending
/// </summary>
public class SimulatedNotificationSender : INotificationSender
{
    private readonly ILogger<SimulatedNotificationSender> _logger;

    public SimulatedNotificationSender(ILogger<SimulatedNotificationSender> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body)
    {
        _logger.LogInformation("📧 [SIMULATED EMAIL] To: {To}, Subject: {Subject}, Body: {Body}", to, subject, body);
        return Task.CompletedTask;
    }

    public Task SendSmsAsync(string phoneNumber, string message)
    {
        _logger.LogInformation("📱 [SIMULATED SMS] To: {PhoneNumber}, Message: {Message}", phoneNumber, message);
        return Task.CompletedTask;
    }

    public Task SendPushAsync(string userId, string title, string message)
    {
        _logger.LogInformation("🔔 [SIMULATED PUSH] To: {UserId}, Title: {Title}, Message: {Message}", userId, title, message);
        return Task.CompletedTask;
    }
}
