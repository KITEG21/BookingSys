namespace Reservation.Domain.Events;

public record ReservationConfirmed(Guid ReservationId, string ClientEmail);