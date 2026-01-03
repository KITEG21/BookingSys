using System;

namespace Reservation.Domain.Entities.Identity;

public class Entity
{
    public Guid Id { get; protected set; } = Guid.CreateVersion7();
    public DateTime CreatedAt { get; protected set; }
    public DateTime UpdatedAt { get; protected set; }

    protected Entity()
    {
        CreatedAt = DateTime.UtcNow;
    }

    protected void Touch() => UpdatedAt = DateTime.UtcNow;
}
