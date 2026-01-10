using System;

namespace Shared.Events;

public record PaymentSettled(
    Guid ReservationId,
    Guid PaymentId,
    DateTime PaidAt
);