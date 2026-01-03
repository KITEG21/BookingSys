using Reservation.Domain.Events;

namespace Reservation.Domain.Saga;

public class ReservationSaga
{
    public Guid ReservationId { get; }
    public ReservationSagaState State { get; private set; }

    public ReservationSaga(Guid reservationId)
    {
        ReservationId = reservationId;
        State = ReservationSagaState.Started;
    }

    public void MarkWaitingForAvailability()
    {
        State = ReservationSagaState.WaitingForAvailability;
    }

    public void Confirm()
    {
        State = ReservationSagaState.Confirmed;
    }

    public void Reject()
    {
        State = ReservationSagaState.Rejected;
    }
    public void Complete()
    {
        State = ReservationSagaState.Completed;
    }
}
