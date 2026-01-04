using Audit.Application.Interfaces;
using Audit.Domain.Entities;
using Audit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Audit.Infrastructure.Repositories;

public class AuditRepository : IAuditRepository
{
    private readonly AuditDbContext _context;

    public AuditRepository(AuditDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditEntry entry)
    {
        _context.AuditEntries.Add(entry);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<AuditEntry>> GetAllAsync(int limit = 100)
    {
        return await _context.AuditEntries
            .OrderByDescending(e => e.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditEntry>> GetByEntityIdAsync(Guid entityId)
    {
        return await _context.AuditEntries
            .Where(e => e.EntityId == entityId)
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditEntry>> GetByEventTypeAsync(string eventType)
    {
        return await _context.AuditEntries
            .Where(e => e.EventType == eventType)
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditEntry>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _context.AuditEntries
            .Where(e => e.Timestamp >= start && e.Timestamp <= end)
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditEntry>> SearchAsync(
        string? eventType, 
        Guid? entityId, 
        DateTime? start, 
        DateTime? end, 
        int limit = 100)
    {
        var query = _context.AuditEntries.AsQueryable();

        if (!string.IsNullOrEmpty(eventType))
            query = query.Where(e => e.EventType == eventType);

        if (entityId.HasValue)
            query = query.Where(e => e.EntityId == entityId);

        if (start.HasValue)
            query = query.Where(e => e.Timestamp >= start.Value);

        if (end.HasValue)
            query = query.Where(e => e.Timestamp <= end.Value);

        return await query
            .OrderByDescending(e => e.Timestamp)
            .Take(limit)
            .ToListAsync();
    }
}
