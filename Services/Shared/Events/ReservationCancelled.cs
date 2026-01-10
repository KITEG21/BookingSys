using System;

namespace Shared.Events;

public record ReservationCancelled(
    Guid ReservationId,
    string ClientEmail
);