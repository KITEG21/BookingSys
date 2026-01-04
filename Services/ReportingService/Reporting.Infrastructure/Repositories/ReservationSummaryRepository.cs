using Microsoft.EntityFrameworkCore;
using Reporting.Application.Interfaces;
using Reporting.Domain.ReadModels;
using Reporting.Infrastructure.Persistence;

namespace Reporting.Infrastructure.Repositories;

public class ReservationSummaryRepository : IReservationSummaryRepository
{
    private readonly ReportingDbContext _context;

    public ReservationSummaryRepository(ReportingDbContext context)
    {
        _context = context;
    }

    public async Task<ReservationSummary?> GetByReservationIdAsync(Guid reservationId)
    {
        return await _context.ReservationSummaries
            .FirstOrDefaultAsync(s => s.ReservationId == reservationId);
    }

    public async Task<IEnumerable<ReservationSummary>> GetAllAsync()
    {
        return await _context.ReservationSummaries
            .OrderByDescending(s => s.LastUpdated)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReservationSummary>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _context.ReservationSummaries
            .Where(s => s.Start >= start && s.Start <= end)
            .OrderBy(s => s.Start)
            .ToListAsync();
    }

    public async Task AddAsync(ReservationSummary summary)
    {
        _context.ReservationSummaries.Add(summary);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ReservationSummary summary)
    {
        _context.ReservationSummaries.Update(summary);
        await _context.SaveChangesAsync();
    }
}
