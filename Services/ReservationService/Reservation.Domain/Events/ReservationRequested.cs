using System;

namespace Reservation.Domain.Events;

public record ReservationRequested
(
    Guid ReservationId,
    Guid ClientId,
    DateTime ReservationDate
);