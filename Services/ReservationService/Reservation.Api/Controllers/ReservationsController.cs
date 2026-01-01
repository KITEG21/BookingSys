using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Reservation.Application.Commands.Post;
using Reservation.Infrastructure.Messaging;

namespace Reservation.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReservationsController : ControllerBase
{
    private readonly CreateReservationCommandHandler _handler;

    public ReservationsController(CreateReservationCommandHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<IActionResult> CreateReservation([FromBody] CreateReservationCommand command)
    {
        try
        {
            var reservation = await _handler.Handle(command);
            return Ok(reservation);
        }
        catch (Exception ex)
        {
            // Log the exception (add ILogger<> to the controller) and return an appropriate response.
            return Problem(detail: ex.Message, statusCode: 500);
        }
    }
}

