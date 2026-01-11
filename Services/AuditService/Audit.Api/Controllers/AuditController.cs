using Microsoft.AspNetCore.Mvc;
using Audit.Application.Services;
using Microsoft.Extensions.Logging;

namespace Audit.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly AuditService _auditService;
    private readonly ILogger<AuditController> _logger;

    public AuditController(AuditService auditService, ILogger<AuditController> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int limit = 100)
    {
        _logger.LogInformation("Retrieving all audit entries with limit {Limit}", limit);
        var entries = await _auditService.SearchAsync(limit: limit);
        _logger.LogInformation("Retrieved {Count} audit entries", entries.Count());
        return Ok(entries);
    }

    [HttpGet("entity/{entityId}")]
    public async Task<IActionResult> GetByEntityId(Guid entityId)
    {
        _logger.LogInformation("Retrieving audit trail for entity {EntityId}", entityId);
        var entries = await _auditService.GetAuditTrailAsync(entityId);
        _logger.LogInformation("Retrieved {Count} audit entries for entity {EntityId}", entries.Count(), entityId);
        return Ok(entries);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? eventType = null,
        [FromQuery] Guid? entityId = null,
        [FromQuery] DateTime? start = null,
        [FromQuery] DateTime? end = null,
        [FromQuery] int limit = 100)
    {
        _logger.LogInformation("Searching audit entries with filters: EventType={EventType}, EntityId={EntityId}, Start={Start}, End={End}, Limit={Limit}", 
            eventType, entityId, start, end, limit);
        var entries = await _auditService.SearchAsync(eventType, entityId, start, end, limit);
        _logger.LogInformation("Search returned {Count} audit entries", entries.Count());
        return Ok(entries);
    }
}
