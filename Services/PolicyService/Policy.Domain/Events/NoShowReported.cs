namespace Policy.Domain.Events;

public record NoShowReported(Guid ReservationId, Guid ClientId);
