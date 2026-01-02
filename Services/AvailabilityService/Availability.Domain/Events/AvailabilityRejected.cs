namespace Availability.Domain.Events;

public record AvailabilityRejected(
    Guid ReservationId
);
