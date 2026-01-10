using System;

namespace Shared.Events;

public record AvailabilityRejected(
    Guid ReservationId
);