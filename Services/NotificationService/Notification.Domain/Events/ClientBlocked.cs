namespace Notification.Domain.Events;

public record ClientBlocked(Guid ClientId, string ClientEmail, string Reason, DateTime BlockedAt);