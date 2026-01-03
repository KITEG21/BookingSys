namespace Payment.Domain.Entities;

public class Payment
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public Guid ReservationId { get; private set; }
    public DateTime PaidAt { get; private set; }
    public bool IsSettled { get; private set; }

    private Payment() { }

    public Payment(Guid reservationId)
    {
        ReservationId = reservationId;
        PaidAt = DateTime.UtcNow;
        IsSettled = true;
    }
}
