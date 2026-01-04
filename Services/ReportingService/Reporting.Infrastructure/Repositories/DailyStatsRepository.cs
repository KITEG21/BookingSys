using Microsoft.EntityFrameworkCore;
using Reporting.Application.Interfaces;
using Reporting.Domain.ReadModels;
using Reporting.Infrastructure.Persistence;

namespace Reporting.Infrastructure.Repositories;

public class DailyStatsRepository : IDailyStatsRepository
{
    private readonly ReportingDbContext _context;

    public DailyStatsRepository(ReportingDbContext context)
    {
        _context = context;
    }

    public async Task<DailyStats?> GetByDateAsync(DateOnly date)
    {
        return await _context.DailyStats
            .FirstOrDefaultAsync(s => s.Date == date);
    }

    public async Task<IEnumerable<DailyStats>> GetByDateRangeAsync(DateOnly start, DateOnly end)
    {
        return await _context.DailyStats
            .Where(s => s.Date >= start && s.Date <= end)
            .OrderBy(s => s.Date)
            .ToListAsync();
    }

    public async Task AddOrUpdateAsync(DailyStats stats)
    {
        var existing = await GetByDateAsync(stats.Date);
        if (existing is null)
        {
            _context.DailyStats.Add(stats);
        }
        else
        {
            existing.TotalReservations = stats.TotalReservations;
            existing.ConfirmedCount = stats.ConfirmedCount;
            existing.CancelledCount = stats.CancelledCount;
            existing.CompletedCount = stats.CompletedCount;
            existing.NoShowCount = stats.NoShowCount;
            existing.OccupancyRate = stats.OccupancyRate;
            existing.LastUpdated = stats.LastUpdated;
        }
        await _context.SaveChangesAsync();
    }
}
