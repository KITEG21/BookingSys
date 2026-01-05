namespace Reservation.Domain.Events;

public record ReservationCancelled(Guid ReservationId, string ClientEmail);