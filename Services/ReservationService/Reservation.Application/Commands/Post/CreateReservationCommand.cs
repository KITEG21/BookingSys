namespace Reservation.Application.Commands.Post;

public record CreateReservationCommand
{
    public Guid ClientId { get; init; }
    public DateTime Start { get; init; }
    public DateTime End { get; init; }

    
}
