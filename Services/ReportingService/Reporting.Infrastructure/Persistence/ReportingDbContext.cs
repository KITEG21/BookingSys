using Microsoft.EntityFrameworkCore;
using Reporting.Domain.ReadModels;

namespace Reporting.Infrastructure.Persistence;

public class ReportingDbContext : DbContext
{
    public ReportingDbContext(DbContextOptions<ReportingDbContext> options) : base(options) { }

    public DbSet<ReservationSummary> ReservationSummaries => Set<ReservationSummary>();
    public DbSet<DailyStats> DailyStats => Set<DailyStats>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReservationSummary>(e =>
        {
            e.ToTable("reservation_summaries");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ReservationId).IsUnique();
            e.HasIndex(x => x.ClientId);
            e.HasIndex(x => x.Start);
            e.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<DailyStats>(e =>
        {
            e.ToTable("daily_stats");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Date).IsUnique();
        });
    }
}
