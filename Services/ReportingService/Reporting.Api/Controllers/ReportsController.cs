using Microsoft.AspNetCore.Mvc;
using Reporting.Application.Queries;

namespace Reporting.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ReportQueries _queries;

    public ReportsController(ReportQueries queries)
    {
        _queries = queries;
    }

    [HttpGet("reservations")]
    public async Task<IActionResult> GetAllReservations()
    {
        var reservations = await _queries.GetAllReservationsAsync();
        return Ok(reservations);
    }

    [HttpGet("reservations/range")]
    public async Task<IActionResult> GetReservationsByDateRange(
        [FromQuery] DateTime start, 
        [FromQuery] DateTime end)
    {
        var reservations = await _queries.GetReservationsByDateRangeAsync(start, end);
        return Ok(reservations);
    }

    [HttpGet("daily-stats")]
    public async Task<IActionResult> GetDailyStats(
        [FromQuery] DateOnly start, 
        [FromQuery] DateOnly end)
    {
        var stats = await _queries.GetDailyStatsAsync(start, end);
        return Ok(stats);
    }

    [HttpGet("occupancy")]
    public async Task<IActionResult> GetOccupancyReport(
        [FromQuery] DateOnly start, 
        [FromQuery] DateOnly end)
    {
        var report = await _queries.GetOccupancyReportAsync(start, end);
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
