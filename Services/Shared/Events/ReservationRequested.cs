using System;

namespace Shared.Events;

public record ReservationRequested(
    Guid ReservationId,
    Guid ClientId,
    DateTime Start,
    DateTime End
);