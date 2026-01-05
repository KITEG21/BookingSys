namespace Notification.Domain.Events;

public record ReservationCompleted(Guid ReservationId, string ClientEmail);