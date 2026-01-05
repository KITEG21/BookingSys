namespace Notification.Domain.Events;

public record PaymentSettled(Guid ReservationId, Guid PaymentId, string ClientEmail, DateTime PaidAt);