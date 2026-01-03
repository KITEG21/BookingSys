using System;
using Reservation.Domain.Entities.Identity;
using Reservation.Domain.Enums;

namespace Reservation.Domain.Entities;

public class Reservation : Entity
{
    public Guid ClientId { get; private set; }
    public DateTime Start { get; private set; }
    public DateTime End { get; private set; }
    public ReservationStatus Status { get; private set; }   

    public Reservation() { }

    public Reservation(Guid clientId, DateTime start, DateTime end) : base()
    {
        ClientId = clientId;
        Start = start.Kind == DateTimeKind.Utc ? start : DateTime.SpecifyKind(start, DateTimeKind.Utc);
        End = end.Kind == DateTimeKind.Utc ? end : DateTime.SpecifyKind(end, DateTimeKind.Utc);
        Status = ReservationStatus.Pending;
    }

    public void Confirm()
    {
        Status = ReservationStatus.Confirmed;
        Touch();
    }

    public void Cancel()
    {
        Status = ReservationStatus.Cancelled;
        Touch();
    }
}
