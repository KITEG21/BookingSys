namespace Availability.Domain.Events;

public record AvailabilityLocked(
    Guid ReservationId,
    DateTime Start,
    DateTime End
);
