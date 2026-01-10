namespace Notification.Domain.Events;

public record ClientBlocked(Guid ClientId, string Reason, DateTime BlockedAt);