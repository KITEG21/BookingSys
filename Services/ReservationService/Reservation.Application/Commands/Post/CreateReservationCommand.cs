namespace Reservation.Application.Commands.Post;

public record CreateReservationCommand
{
    public Guid ClientId { get; init; }
    public DateTime ReservationDate { get; init; }
    
}
