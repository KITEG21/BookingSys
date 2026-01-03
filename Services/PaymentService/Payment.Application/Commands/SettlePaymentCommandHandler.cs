using Payment.Application.Commands;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;
using Payment.Domain.Events;

namespace Payment.Application.Handlers;

public class SettlePaymentCommandHandler
{
    private readonly IEventBus _eventBus;

    public SettlePaymentCommandHandler(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task<Domain.Entities.Payment> Handle(SettlePaymentCommand command)
    {
        // Crear entidad de pago (manual)
        var payment = new Domain.Entities.Payment(command.ReservationId);

        // Publicar evento
        await _eventBus.PublishAsync(new PaymentSettled(
            command.ReservationId,
            payment.Id,
            payment.PaidAt
        ));

        return payment;
    }
}
