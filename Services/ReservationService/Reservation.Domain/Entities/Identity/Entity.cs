using System;

namespace Reservation.Domain.Entities.Identity;

public class Entity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime UpdatedAt { get; protected set; }

    protected Entity()
    {
        var now = DateTime.UtcNow;
        CreatedAt = now;
        UpdatedAt = now;
    }

    protected void Touch() => UpdatedAt = DateTime.UtcNow;
}
