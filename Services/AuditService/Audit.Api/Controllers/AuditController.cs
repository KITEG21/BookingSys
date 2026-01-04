using Microsoft.AspNetCore.Mvc;
using Audit.Application.Services;

namespace Audit.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly AuditService _auditService;

    public AuditController(AuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int limit = 100)
    {
        var entries = await _auditService.SearchAsync(limit: limit);
        return Ok(entries);
    }

    [HttpGet("entity/{entityId}")]
    public async Task<IActionResult> GetByEntityId(Guid entityId)
    {
        var entries = await _auditService.GetAuditTrailAsync(entityId);
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
        var entries = await _auditService.SearchAsync(eventType, entityId, start, end, limit);
        return Ok(entries);
    }
}
