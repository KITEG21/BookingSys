namespace Reservation.Domain.Saga;

public enum ReservationSagaState
{
    Started,
    WaitingForAvailability,
    Confirmed,
    Rejected
}
