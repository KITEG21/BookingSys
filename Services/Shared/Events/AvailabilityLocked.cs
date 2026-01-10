using System;

namespace Shared.Events;

public record AvailabilityLocked(
    Guid ReservationId,
    DateTime Start,
    DateTime End
);