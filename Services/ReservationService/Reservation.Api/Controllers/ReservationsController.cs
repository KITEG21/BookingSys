using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Reservation.Application.Commands.Post;
using Reservation.Application.Queries.GetAll;
using Reservation.Application.Queries.GetById;
using Reservation.Infrastructure.Messaging;

namespace Reservation.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ReservationsController : ControllerBase
{
    private readonly CreateReservationCommandHandler _createHandler;
    private readonly GetReservationQueryHandler _getHandler;
    private readonly GetAllReservationsQueryHandler _getAllHandler;
    private readonly ILogger<ReservationsController> _logger;

    public ReservationsController(
        CreateReservationCommandHandler createHandler,
        GetReservationQueryHandler getHandler,
        GetAllReservationsQueryHandler getAllHandler,
        ILogger<ReservationsController> logger)
    {
        _createHandler = createHandler;
        _getHandler = getHandler;
        _getAllHandler = getAllHandler;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateReservation([FromBody] CreateReservationCommand command)
    {
        _logger.LogInformation("Booking attempt for user {UserId}", command.ClientId);

        try
        {
            var reservation = await _createHandler.Handle(command);
            _logger.LogInformation("Booking {BookingId} created for user {UserId}", reservation.Id, reservation.ClientId);
            return Ok(reservation);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Booking failed for user {UserId}", command.ClientId);
            return Problem(detail: ex.Message, statusCode: 500);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _getAllHandler.Handle(new GetAllReservationsQuery());
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        _logger.LogInformation("Retrieving reservation by for reservation {ReservationId}", id);
        var r = await _getHandler.Handle(new GetReservationQuery(id));
        return r is null ? NotFound() : Ok(r);
    }
}

