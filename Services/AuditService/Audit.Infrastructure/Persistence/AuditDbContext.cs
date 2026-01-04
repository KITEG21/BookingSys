using Audit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Audit.Infrastructure.Persistence;

public class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }

    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditEntry>(e =>
        {
            e.ToTable("audit_entries");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.EventType);
            e.HasIndex(x => x.EntityId);
            e.HasIndex(x => x.Timestamp);
            e.HasIndex(x => x.SourceService);
            e.HasIndex(x => x.CorrelationId);
        });
    }
}
