using Microsoft.Extensions.Logging;
using Reporting.Application.Interfaces;
using Reporting.Domain.Events;
using Reporting.Domain.ReadModels;

namespace Reporting.Application.Services;

public class ReportingProjectionService
{
    private readonly IReservationSummaryRepository _summaryRepository;
    private readonly IDailyStatsRepository _statsRepository;
    private readonly ILogger<ReportingProjectionService> _logger;

    public ReportingProjectionService(
        IReservationSummaryRepository summaryRepository,
        IDailyStatsRepository statsRepository,
        ILogger<ReportingProjectionService> logger)
    {
        _summaryRepository = summaryRepository;
        _statsRepository = statsRepository;
        _logger = logger;
    }

    public async Task HandleReservationRequestedAsync(ReservationRequested evt)
    {
        _logger.LogInformation("Projecting ReservationRequested: {ReservationId}", evt.ReservationId);

        // IDEMPOTENCY: Check if already exists
        var existing = await _summaryRepository.GetByReservationIdAsync(evt.ReservationId);
        if (existing != null)
        {
            _logger.LogInformation("ReservationSummary already exists for {ReservationId}, skipping", evt.ReservationId);
            return;
        }

        var summary = new ReservationSummary
        {
            ReservationId = evt.ReservationId,
            ClientId = evt.ClientId,
            Start = DateTime.SpecifyKind(evt.Start, DateTimeKind.Utc),
            End = DateTime.SpecifyKind(evt.End, DateTimeKind.Utc),
            Status = "Pending",
            LastUpdated = DateTime.UtcNow
        };

        await _summaryRepository.AddAsync(summary);
        await UpdateDailyStatsAsync(DateOnly.FromDateTime(evt.Start));
    }

    public async Task HandleReservationConfirmedAsync(ReservationConfirmed evt)
    {
        _logger.LogInformation("Projecting ReservationConfirmed: {ReservationId}", evt.ReservationId);

        var summary = await _summaryRepository.GetByReservationIdAsync(evt.ReservationId);
        if (summary is null)
        {
            _logger.LogWarning("ReservationSummary not found for {ReservationId}", evt.ReservationId);
            return;
        }

        summary.Status = "Confirmed";
        summary.ConfirmedAt = DateTime.UtcNow;
        summary.LastUpdated = DateTime.UtcNow;

        await _summaryRepository.UpdateAsync(summary);
        await UpdateDailyStatsAsync(DateOnly.FromDateTime(summary.Start));
    }

    public async Task HandleReservationCancelledAsync(ReservationCancelled evt)
    {
        _logger.LogInformation("Projecting ReservationCancelled: {ReservationId}", evt.ReservationId);

        var summary = await _summaryRepository.GetByReservationIdAsync(evt.ReservationId);
        if (summary is null)
        {
            _logger.LogWarning("ReservationSummary not found for {ReservationId}", evt.ReservationId);
            return;
        }

        summary.Status = "Cancelled";
        summary.CancelledAt = DateTime.UtcNow;
        summary.LastUpdated = DateTime.UtcNow;

        await _summaryRepository.UpdateAsync(summary);
        await UpdateDailyStatsAsync(DateOnly.FromDateTime(summary.Start));
    }

    public async Task HandleReservationCompletedAsync(ReservationCompleted evt)
    {
        _logger.LogInformation("Projecting ReservationCompleted: {ReservationId}", evt.ReservationId);

        var summary = await _summaryRepository.GetByReservationIdAsync(evt.ReservationId);
        if (summary is null)
        {
            _logger.LogWarning("ReservationSummary not found for {ReservationId}", evt.ReservationId);
            return;
        }

        summary.Status = "Completed";
        summary.CompletedAt = DateTime.UtcNow;
        summary.LastUpdated = DateTime.UtcNow;

        await _summaryRepository.UpdateAsync(summary);
        await UpdateDailyStatsAsync(DateOnly.FromDateTime(summary.Start));
    }

    public async Task HandlePaymentSettledAsync(PaymentSettled evt)
    {
        _logger.LogInformation("Projecting PaymentSettled: {ReservationId}", evt.ReservationId);

        var summary = await _summaryRepository.GetByReservationIdAsync(evt.ReservationId);
        if (summary is null)
        {
            _logger.LogWarning("ReservationSummary not found for {ReservationId}", evt.ReservationId);
            return;
        }

        summary.PaymentId = evt.PaymentId;
        summary.PaidAt = DateTime.SpecifyKind(evt.PaidAt, DateTimeKind.Utc);
        summary.LastUpdated = DateTime.UtcNow;

        await _summaryRepository.UpdateAsync(summary);
    }

    private async Task UpdateDailyStatsAsync(DateOnly date)
    {
        // FIX: Convert DateOnly to UTC DateTime properly
        var startUtc = DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var endUtc = DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);
        
        var summaries = await _summaryRepository.GetByDateRangeAsync(startUtc, endUtc);

        var list = summaries.ToList();
        var stats = await _statsRepository.GetByDateAsync(date) ?? new DailyStats { Date = date };

        stats.TotalReservations = list.Count;
        stats.ConfirmedCount = list.Count(s => s.Status == "Confirmed" || s.Status == "Completed");
        stats.CancelledCount = list.Count(s => s.Status == "Cancelled");
        stats.CompletedCount = list.Count(s => s.Status == "Completed");
        stats.OccupancyRate = stats.TotalReservations > 0 
            ? (decimal)stats.ConfirmedCount / stats.TotalReservations * 100 
            : 0;
        stats.LastUpdated = DateTime.UtcNow;

        await _statsRepository.AddOrUpdateAsync(stats);
    }
}