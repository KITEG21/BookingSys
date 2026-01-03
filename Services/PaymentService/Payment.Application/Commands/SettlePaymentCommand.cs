namespace Payment.Application.Commands;

public record SettlePaymentCommand(
    Guid ReservationId
);