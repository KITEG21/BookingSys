using Microsoft.AspNetCore.Mvc;
using Payment.Application.Commands;
using Payment.Application.Handlers;

namespace Payment.Api.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly SettlePaymentCommandHandler _handler;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(SettlePaymentCommandHandler handler, ILogger<PaymentsController> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    [HttpPost("settle")]
    public async Task<IActionResult> Settle([FromBody] SettlePaymentCommand command)
    {
        _logger.LogInformation("Settling payment for reservation: {ReservationId}", command.ReservationId);
        var payment = await _handler.Handle(command);
        return Ok(payment);
    }
}
