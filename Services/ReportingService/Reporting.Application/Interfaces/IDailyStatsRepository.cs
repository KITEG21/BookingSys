using Reporting.Domain.ReadModels;

namespace Reporting.Application.Interfaces;

public interface IDailyStatsRepository
{
    Task<DailyStats?> GetByDateAsync(DateOnly date);
    Task<IEnumerable<DailyStats>> GetByDateRangeAsync(DateOnly start, DateOnly end);
    Task AddOrUpdateAsync(DailyStats stats);
}
