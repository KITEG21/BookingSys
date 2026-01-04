namespace Reporting.Domain.ReadModels;

/// <summary>
/// Aggregated daily statistics
/// </summary>
public class DailyStats
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateOnly Date { get; set; }
    public int TotalReservations { get; set; }
    public int ConfirmedCount { get; set; }
    public int CancelledCount { get; set; }
    public int CompletedCount { get; set; }
    public int NoShowCount { get; set; }
    public decimal OccupancyRate { get; set; }
    public DateTime LastUpdated { get; set; }
}
