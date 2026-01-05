namespace Notification.Domain.Events;

public record ReservationConfirmed(Guid ReservationId, string ClientEmail);