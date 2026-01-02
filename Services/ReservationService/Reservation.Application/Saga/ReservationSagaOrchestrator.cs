using Reservation.Application.Interfaces;
using Reservation.Domain.Events;
using Reservation.Domain.Saga;

namespace Reservation.Application.Saga;

public class ReservationSagaOrchestrator
{
    private static readonly Dictionary<Guid, ReservationSaga> _sagas = new();

    private readonly IEventBus _eventBus;

    public ReservationSagaOrchestrator(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    // public async Task StartAsync(Guid reservationId)
    // {
    //     var saga = new ReservationSaga(reservationId);
    //     saga.MarkWaitingForAvailability();

    //     _sagas[reservationId] = saga;

    //     await _eventBus.PublishAsync(
    //         new ReservationRequested(reservationId)
    //     );
    // }

    public async Task HandleAsync(AvailabilityLocked @event)
    {
        if (!_sagas.TryGetValue(@event.ReservationId, out var saga))
            return;

        saga.Confirm();

        await _eventBus.PublishAsync(
            new ReservationConfirmed(@event.ReservationId)
        );
    }

    public async Task HandleAsync(AvailabilityRejected @event)
    {
        if (!_sagas.TryGetValue(@event.ReservationId, out var saga))
            return;

        saga.Reject();

        await _eventBus.PublishAsync(
            new ReservationCancelled(@event.ReservationId)
        );
    }
}
