using System;

namespace Shared.Events;

public record ReservationConfirmed(
    Guid ReservationId,
    string ClientEmail
);