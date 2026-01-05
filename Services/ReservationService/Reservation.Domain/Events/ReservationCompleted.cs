namespace Reservation.Domain.Events;

public record ReservationCompleted(Guid ReservationId, string ClientEmail);