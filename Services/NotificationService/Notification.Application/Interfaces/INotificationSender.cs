namespace Notification.Application.Interfaces;

public interface INotificationSender
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendSmsAsync(string phoneNumber, string message);
    Task SendPushAsync(string userId, string title, string message);
}
