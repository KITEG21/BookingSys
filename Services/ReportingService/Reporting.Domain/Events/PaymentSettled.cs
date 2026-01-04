namespace Reporting.Domain.Events;

public record PaymentSettled(Guid ReservationId, Guid PaymentId, DateTime PaidAt);
