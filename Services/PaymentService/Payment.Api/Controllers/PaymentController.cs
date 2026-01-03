using Microsoft.AspNetCore.Mvc;
using Payment.Application.Commands;
using Payment.Application.Handlers;

namespace Payment.Api.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly SettlePaymentCommandHandler _handler;

    public PaymentsController(SettlePaymentCommandHandler handler)
    {
        _handler = handler;
    }

    [HttpPost("settle")]
    public async Task<IActionResult> Settle([FromBody] SettlePaymentCommand command)
    {
        var payment = await _handler.Handle(command);
        return Ok(payment);
    }
}
