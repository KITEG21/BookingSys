using System;

namespace Availability.Domain.Events;

public record ReservationRequested
(
    Guid ReservationId,
    Guid ClientId,
    DateTime Start,
    DateTime End
);