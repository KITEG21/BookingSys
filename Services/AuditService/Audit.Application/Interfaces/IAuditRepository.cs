using Audit.Domain.Entities;

namespace Audit.Application.Interfaces;

public interface IAuditRepository
{
    Task AddAsync(AuditEntry entry);
    Task<IEnumerable<AuditEntry>> GetAllAsync(int limit = 100);
    Task<IEnumerable<AuditEntry>> GetByEntityIdAsync(Guid entityId);
    Task<IEnumerable<AuditEntry>> GetByEventTypeAsync(string eventType);
    Task<IEnumerable<AuditEntry>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<IEnumerable<AuditEntry>> SearchAsync(string? eventType, Guid? entityId, DateTime? start, DateTime? end, int limit = 100);
}
