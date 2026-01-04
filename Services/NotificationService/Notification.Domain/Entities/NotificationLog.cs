namespace Notification.Domain.Entities;

public class NotificationLog
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public string Channel { get; private set; } = string.Empty; // Email, SMS, Push
    public string Recipient { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public string EventType { get; private set; } = string.Empty;
    public Guid? RelatedEntityId { get; private set; }
    public DateTime SentAt { get; private set; }
    public bool IsSuccess { get; private set; }

    private NotificationLog() { }

    public NotificationLog(string channel, string recipient, string subject, string body, string eventType, Guid? relatedEntityId = null)
    {
        Channel = channel;
        Recipient = recipient;
        Subject = subject;
        Body = body;
        EventType = eventType;
        RelatedEntityId = relatedEntityId;
        SentAt = DateTime.UtcNow;
        IsSuccess = true;
    }

    public void MarkFailed()
    {
        IsSuccess = false;
    }
}
