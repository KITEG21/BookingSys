namespace Reporting.Domain.ReadModels;

/// <summary>
/// Denormalized read model for reservation statistics
/// </summary>
public class ReservationSummary
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid ReservationId { get; set; }
    public Guid ClientId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public Guid? PaymentId { get; set; }
    public DateTime LastUpdated { get; set; }
}
