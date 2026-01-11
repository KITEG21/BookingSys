using Microsoft.AspNetCore.Mvc;
using Reporting.Application.Queries;
using Microsoft.Extensions.Logging;

namespace Reporting.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ReportQueries _queries;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(ReportQueries queries, ILogger<ReportsController> logger)
    {
        _queries = queries;
        _logger = logger;
    }

    [HttpGet("reservations")]
    public async Task<IActionResult> GetAllReservations()
    {
        _logger.LogInformation("Retrieving all reservations report");
        var reservations = await _queries.GetAllReservationsAsync();
        _logger.LogInformation("Retrieved {Count} reservations", reservations.Count());
        return Ok(reservations);
    }

    [HttpGet("reservations/range")]
    public async Task<IActionResult> GetReservationsByDateRange(
        [FromQuery] DateTime start, 
        [FromQuery] DateTime end)
    {
        _logger.LogInformation("Retrieving reservations report for date range {Start} to {End}", start, end);
        var reservations = await _queries.GetReservationsByDateRangeAsync(start, end);
        _logger.LogInformation("Retrieved {Count} reservations in range", reservations.Count());
        return Ok(reservations);
    }

    [HttpGet("daily-stats")]
    public async Task<IActionResult> GetDailyStats(
        [FromQuery] DateOnly start, 
        [FromQuery] DateOnly end)
    {
        _logger.LogInformation("Retrieving daily stats report for {Start} to {End}", start, end);
        var stats = await _queries.GetDailyStatsAsync(start, end);
        _logger.LogInformation("Retrieved daily stats for {Days} days", stats.Count());
        return Ok(stats);
    }

    [HttpGet("occupancy")]
    public async Task<IActionResult> GetOccupancyReport(
        [FromQuery] DateOnly start, 
        [FromQuery] DateOnly end)
    {
        _logger.LogInformation("Retrieving occupancy report for {Start} to {End}", start, end);
        var report = await _queries.GetOccupancyReportAsync(start, end);
        _logger.LogInformation("Retrieved occupancy report");
        return Ok(report);
    }

    [HttpGet("cancellations")]
    public async Task<IActionResult> GetCancellationReport(
        [FromQuery] DateOnly start, 
        [FromQuery] DateOnly end)
    {
        var report = await _queries.GetCancellationReportAsync(start, end);
        return Ok(report);
    }
}
