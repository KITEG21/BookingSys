using System;
using Reservation.Domain.Entities.Identity;
using Reservation.Domain.Enums;

namespace Reservation.Domain.Entities;

public class Reservation : Entity
{
    public Guid ClientId { get; private set; }
    public DateTime ReservationDate { get; private set; }
    public ReservationStatus Status { get; private set; }   


    public Reservation(Guid clientId, DateTime reservationDate)
    {
        ClientId = clientId;
        ReservationDate = reservationDate;
        Status = ReservationStatus.Pending;
    }

    public void Confirm()
    {
        Status = ReservationStatus.Confirmed;
    }

    public void Cancel()
    {
        Status = ReservationStatus.Cancelled;
    }
}
