namespace Policy.Domain.Events;

public record PenaltyApplied(Guid ClientId, Guid ReservationId, string PenaltyType, string Description);
