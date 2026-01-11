using Audit.Application.Interfaces;
using Audit.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Audit.Application.Services;

public class AuditService
{
    private readonly IAuditRepository _repository;
    private readonly ILogger<AuditService> _logger;

    public AuditService(IAuditRepository repository, ILogger<AuditService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task RecordEventAsync(
        string eventType, 
        string eventData, 
        Guid? entityId = null,
        string? entityType = null,
        string? actor = null,
        string? sourceService = null,
        string? correlationId = null)
    {
        var entry = new AuditEntry(
            eventType, 
            eventData, 
            entityId, 
            entityType, 
            actor, 
            sourceService,
            correlationId);

        await _repository.AddAsync(entry);

        _logger.LogInformation("Audit recorded: {EventType} for {EntityType}:{EntityId}", 
            eventType, entityType, entityId);
    }

    public async Task<IEnumerable<AuditEntry>> GetAuditTrailAsync(Guid entityId)
    {
        _logger.LogDebug("Retrieving audit trail for entity {EntityId}", entityId);
        var entries = await _repository.GetByEntityIdAsync(entityId);
        _logger.LogDebug("Retrieved {Count} entries for entity {EntityId}", entries.Count(), entityId);
        return entries;
    }

    public async Task<IEnumerable<AuditEntry>> SearchAsync(
        string? eventType = null, 
        Guid? entityId = null, 
        DateTime? start = null, 
        DateTime? end = null,
        int limit = 100)
    {
        _logger.LogDebug("Searching audit entries with filters: EventType={EventType}, EntityId={EntityId}, Start={Start}, End={End}, Limit={Limit}", 
            eventType, entityId, start, end, limit);
        var entries = await _repository.SearchAsync(eventType, entityId, start, end, limit);
        _logger.LogDebug("Search returned {Count} entries", entries.Count());
        return entries;
    }
}
