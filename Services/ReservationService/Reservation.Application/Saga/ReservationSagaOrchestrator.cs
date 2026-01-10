using Shared.Interfaces;
using Shared.Events;
using Reservation.Application.Interfaces;
using Reservation.Domain.Saga;
using Reservation.Domain.Entities;

namespace Reservation.Application.Saga;

public class ReservationSagaOrchestrator
{
    private readonly IEventBus _eventBus;
    private readonly ISagaRepository _sagaRepository;
    private readonly IReservationRepository _reservationRepository;

    public ReservationSagaOrchestrator(
        IEventBus eventBus,
        ISagaRepository sagaRepository,
        IReservationRepository reservationRepository)
    {
        _eventBus = eventBus;
        _sagaRepository = sagaRepository;
        _reservationRepository = reservationRepository;
    }

    public async Task StartAsync(Domain.Entities.Reservation reservation)
    {
        var saga = new ReservationSaga(reservation.Id);
        saga.MarkWaitingForAvailability();
        await _sagaRepository.CreateAsync(saga);

        await _eventBus.PublishAsync(new ReservationRequested(
            reservation.Id, reservation.ClientId, reservation.Start, reservation.End));
    }

    public async Task HandleAsync(AvailabilityLocked @event)
    {
        var saga = await _sagaRepository.GetAsync(@event.ReservationId);
        if (saga is null) return;
        if (saga.State != ReservationSagaState.WaitingForAvailability) return;

        saga.Confirm();
        await _sagaRepository.UpdateAsync(saga);

        var reservation = await _reservationRepository.GetAsync(@event.ReservationId);
        if (reservation != null)
        {
            reservation.Confirm();
            await _reservationRepository.UpdateAsync(reservation);
            await _eventBus.PublishAsync(new ReservationConfirmed(reservation.Id, reservation.ClientEmail));
        }
    }

    public async Task HandleAsync(AvailabilityRejected @event)
    {
        var saga = await _sagaRepository.GetAsync(@event.ReservationId);
        if (saga is null) return;
        if (saga.State != ReservationSagaState.WaitingForAvailability) return;

        saga.Reject();
        await _sagaRepository.UpdateAsync(saga);

        var reservation = await _reservationRepository.GetAsync(@event.ReservationId);
        if (reservation != null)
        {
            reservation.Cancel();
            await _reservationRepository.UpdateAsync(reservation);
            await _eventBus.PublishAsync(new ReservationCancelled(reservation.Id, reservation.ClientEmail));
        }
    }

    public async Task HandleAsync(PaymentSettled @event)
    {
        var saga = await _sagaRepository.GetAsync(@event.ReservationId);
        if (saga is null) return;
        if (saga.State == ReservationSagaState.Completed) return;

        saga.Complete();
        await _sagaRepository.UpdateAsync(saga);

        var reservation = await _reservationRepository.GetAsync(@event.ReservationId);
        if (reservation != null)
        {
            reservation.Complete();
            await _reservationRepository.UpdateAsync(reservation);
            await _eventBus.PublishAsync(new ReservationCompleted(reservation.Id, reservation.ClientEmail));
        }
    }
}