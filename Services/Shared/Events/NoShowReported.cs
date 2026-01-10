using System;

namespace Shared.Events;

public record NoShowReported(
    Guid ReservationId,
    Guid ClientId
);