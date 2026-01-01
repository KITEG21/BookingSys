using System;
using System.Threading.Tasks;
using Reservation.Domain.Events;
using Reservation.Infrastructure.Messaging;

namespace Reservation.Application.Commands.Post;

public class CreateReservationCommandHandler
{
    private readonly IEventBus _eventBus;
    public CreateReservationCommandHandler(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task<Reservation.Domain.Entities.Reservation> Handle(CreateReservationCommand command)
    {
        var reservation = new Reservation.Domain.Entities.Reservation(command.ClientId, command.ReservationDate);

        await _eventBus.PublishAsync(new ReservationRequested(
            reservation.Id,
            reservation.ClientId,
            reservation.ReservationDate
        ));
        
        return reservation;
    }
}
