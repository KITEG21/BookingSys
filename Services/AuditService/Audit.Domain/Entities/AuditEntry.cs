namespace Audit.Domain.Entities;

/// <summary>
/// Immutable audit log entry - stores all events for audit trail
/// </summary>
public class AuditEntry
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public string EventType { get; private set; } = string.Empty;
    public string EventData { get; private set; } = string.Empty;
    public Guid? EntityId { get; private set; }
    public string? EntityType { get; private set; }
    public string? Actor { get; private set; } // Could be service name, user id, etc.
    public string SourceService { get; private set; } = string.Empty;
    public DateTime Timestamp { get; private set; }
    public string? CorrelationId { get; private set; }

    private AuditEntry() { }

    public AuditEntry(
        string eventType, 
        string eventData, 
        Guid? entityId = null,
        string? entityType = null,
        string? actor = null,
        string? sourceService = null,
        string? correlationId = null)
    {
        EventType = eventType;
        EventData = eventData;
        EntityId = entityId;
        EntityType = entityType;
        Actor = actor;
        SourceService = sourceService ?? "Unknown";
        Timestamp = DateTime.UtcNow;
        CorrelationId = correlationId;
    }
}
