namespace Policy.Domain.Entities;

public class ClientViolation
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public Guid ClientId { get; private set; }
    public string ViolationType { get; private set; } = string.Empty; // NoShow, LateCancellation
    public Guid ReservationId { get; private set; }
    public DateTime OccurredAt { get; private set; }

    private ClientViolation() { }

    public ClientViolation(Guid clientId, string violationType, Guid reservationId)
    {
        ClientId = clientId;
        ViolationType = violationType;
        ReservationId = reservationId;
        OccurredAt = DateTime.UtcNow;
    }
}
