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

    public ReservationsController(
        CreateReservationCommandHandler createHandler,
        GetReservationQueryHandler getHandler,
        GetAllReservationsQueryHandler getAllHandler)
    {
        _createHandler = createHandler;
        _getHandler = getHandler;
        _getAllHandler = getAllHandler;
    }

    [HttpPost]
    public async Task<IActionResult> CreateReservation([FromBody] CreateReservationCommand command)
    {
        try
        {
            var reservation = await _createHandler.Handle(command);
            return Ok(reservation);
        }
        catch (Exception ex)
        {
            // Log the exception (add ILogger<> to the controller) and return an appropriate response.
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
        var r = await _getHandler.Handle(new GetReservationQuery(id));
        return r is null ? NotFound() : Ok(r);
    }
}

