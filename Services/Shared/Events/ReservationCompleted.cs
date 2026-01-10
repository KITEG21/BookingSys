using System;

namespace Shared.Events;

public record ReservationCompleted(
    Guid ReservationId,
    string ClientEmail
);