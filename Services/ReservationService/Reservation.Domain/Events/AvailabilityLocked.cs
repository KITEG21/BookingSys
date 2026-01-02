namespace Reservation.Domain.Events;

public record AvailabilityLocked(Guid ReservationId, DateTime Start, DateTime End);
