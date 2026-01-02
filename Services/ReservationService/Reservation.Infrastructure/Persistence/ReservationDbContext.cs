using Microsoft.EntityFrameworkCore;
using Reservation.Domain.Entities;

namespace Reservation.Infrastructure.Persistence;

public class ReservationDbContext : DbContext
{
    public DbSet<Domain.Entities.Reservation> Reservations { get; set; } = null!;
    public DbSet<ReservationSagaEntity> ReservationSagas { get; set; } = null!;

    public ReservationDbContext(DbContextOptions<ReservationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Domain.Entities.Reservation>(b =>
        {
            b.ToTable("reservations");
            b.HasKey(x => x.Id);
            b.Property(x => x.ClientId).IsRequired();
            b.Property(x => x.Start).IsRequired();
            b.Property(x => x.End).IsRequired();
            b.Property(x => x.Status).HasConversion<string>().IsRequired();
        });

        builder.Entity<ReservationSagaEntity>(b =>
        {
            b.ToTable("reservation_sagas");
            b.HasKey(x => x.ReservationId);
            b.Property(x => x.State).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
        });
    }
}

// DTO entity for saga persistence
public class ReservationSagaEntity
{
    public Guid ReservationId { get; set; }
    public string State { get; set; } = null!;
    public DateTime UpdatedAt { get; set; }
}