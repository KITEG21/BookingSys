using System;
using System.Threading.Tasks;
using Reservation.Application.Interfaces;
using Reservation.Application.Saga;
using Reservation.Domain.Events;

namespace Reservation.Application.Commands.Post;

public class CreateReservationCommandHandler
{
    private readonly IReservationRepository _repository;
    private readonly ReservationSagaOrchestrator _orchestrator;

    public CreateReservationCommandHandler(IReservationRepository repository, ReservationSagaOrchestrator orchestrator)
    {
        _repository = repository;
        _orchestrator = orchestrator;
    }

    public async Task<Domain.Entities.Reservation> Handle(CreateReservationCommand command)
    {
        var reservation = new Domain.Entities.Reservation(command.ClientId, command.Start, command.End);

        await _repository.AddAsync(reservation);

        // Inicia el Saga (persiste el state y publica ReservationRequested internamente)
        await _orchestrator.StartAsync(reservation);

        return reservation;
    }
}
