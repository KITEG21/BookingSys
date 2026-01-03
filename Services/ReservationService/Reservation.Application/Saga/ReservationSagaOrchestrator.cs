using Reservation.Application.Interfaces;
using Reservation.Domain.Events;
using Reservation.Domain.Saga;
using Reservation.Domain.Entities;

namespace Reservation.Application.Saga;

public class ReservationSagaOrchestrator
{
    private readonly IEventBus _eventBus;
    private readonly ISagaRepository _sagaRepository;

    public ReservationSagaOrchestrator(IEventBus eventBus, ISagaRepository sagaRepository)
    {
        _eventBus = eventBus;
        _sagaRepository = sagaRepository;
    }

    // Ahora recibe la entidad Reservation para publicar los datos completos
    public async Task StartAsync(Domain.Entities.Reservation reservation)
    {
        var saga = new ReservationSaga(reservation.Id);
        saga.MarkWaitingForAvailability();

        await _sagaRepository.CreateAsync(saga);

        await _eventBus.PublishAsync(
            new ReservationRequested(reservation.Id, reservation.ClientId, reservation.Start, reservation.End)
        );
    }

    public async Task HandleAsync(AvailabilityLocked @event)
    {
        var saga = await _sagaRepository.GetAsync(@event.ReservationId);
        if (saga is null) return;

        saga.Confirm();
        await _sagaRepository.UpdateAsync(saga);

        await _eventBus.PublishAsync(new ReservationConfirmed(@event.ReservationId));
    }

    public async Task HandleAsync(AvailabilityRejected @event)
    {
        var saga = await _sagaRepository.GetAsync(@event.ReservationId);
        if (saga is null) return;

        saga.Reject();
        await _sagaRepository.UpdateAsync(saga);

        await _eventBus.PublishAsync(new ReservationCancelled(@event.ReservationId));
    }
}