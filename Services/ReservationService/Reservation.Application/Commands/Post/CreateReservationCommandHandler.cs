using System;
using System.Threading.Tasks;
using Reservation.Application.Interfaces;
using Reservation.Application.Saga;
using Shared.Events;

namespace Reservation.Application.Commands.Post;

public class CreateReservationCommandHandler
{
    private readonly IReservationRepository _repository;
    private readonly ReservationSagaOrchestrator _orchestrator;
    private readonly IUserContext _userContext;

    public CreateReservationCommandHandler(IReservationRepository repository, ReservationSagaOrchestrator orchestrator, IUserContext userContext)
    {
        _repository = repository;
        _orchestrator = orchestrator;
        _userContext = userContext;
    }

    public async Task<Domain.Entities.Reservation> Handle(CreateReservationCommand command)
    {
        var userId = _userContext.GetUserId() 
            ?? throw new InvalidOperationException("User must be authenticated to create a reservation");
        var userEmail = _userContext.GetCurrentUserEmail() 
            ?? throw new InvalidOperationException("User email is required to create a reservation");
        var reservation = new Domain.Entities.Reservation(userId, userEmail, command.Start, command.End);

        await _repository.AddAsync(reservation);

        // Inicia el Saga (persiste el state y publica ReservationRequested internamente)
        await _orchestrator.StartAsync(reservation);

        return reservation;
    }
}
