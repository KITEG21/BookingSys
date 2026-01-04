using Reporting.Application.Interfaces;
using Reporting.Domain.ReadModels;

namespace Reporting.Application.Queries;

public class ReportQueries
{
    private readonly IReservationSummaryRepository _summaryRepository;
    private readonly IDailyStatsRepository _statsRepository;

    public ReportQueries(
        IReservationSummaryRepository summaryRepository,
        IDailyStatsRepository statsRepository)
    {
        _summaryRepository = summaryRepository;
        _statsRepository = statsRepository;
    }

    public async Task<IEnumerable<ReservationSummary>> GetAllReservationsAsync()
    {
        return await _summaryRepository.GetAllAsync();
    }

    public async Task<IEnumerable<ReservationSummary>> GetReservationsByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _summaryRepository.GetByDateRangeAsync(start, end);
    }

    public async Task<IEnumerable<DailyStats>> GetDailyStatsAsync(DateOnly start, DateOnly end)
    {
        return await _statsRepository.GetByDateRangeAsync(start, end);
    }

    public async Task<object> GetOccupancyReportAsync(DateOnly start, DateOnly end)
    {
        var stats = await _statsRepository.GetByDateRangeAsync(start, end);
        var list = stats.ToList();

        return new
        {
            Period = new { Start = start, End = end },
            TotalDays = list.Count,
            TotalReservations = list.Sum(s => s.TotalReservations),
            TotalConfirmed = list.Sum(s => s.ConfirmedCount),
            TotalCancelled = list.Sum(s => s.CancelledCount),
            TotalCompleted = list.Sum(s => s.CompletedCount),
            TotalNoShows = list.Sum(s => s.NoShowCount),
            AverageOccupancyRate = list.Count > 0 ? list.Average(s => s.OccupancyRate) : 0,
            DailyBreakdown = list
        };
    }

    public async Task<object> GetCancellationReportAsync(DateOnly start, DateOnly end)
    {
        var summaries = await _summaryRepository.GetByDateRangeAsync(
            start.ToDateTime(TimeOnly.MinValue),
            end.ToDateTime(TimeOnly.MaxValue));

        var cancelled = summaries.Where(s => s.Status == "Cancelled").ToList();

        return new
        {
            Period = new { Start = start, End = end },
            TotalCancellations = cancelled.Count,
            CancellationsByDay = cancelled
                .GroupBy(c => DateOnly.FromDateTime(c.CancelledAt ?? c.LastUpdated))
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
        };
    }
}
