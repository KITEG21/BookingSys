using Microsoft.EntityFrameworkCore;
using Policy.Domain.Entities;

namespace Policy.Infrastructure.Persistence;

public class PolicyDbContext : DbContext
{
    public PolicyDbContext(DbContextOptions<PolicyDbContext> options) : base(options) { }

    public DbSet<ClientViolation> Violations => Set<ClientViolation>();
    public DbSet<ClientBlock> Blocks => Set<ClientBlock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClientViolation>(e =>
        {
            e.ToTable("client_violations");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ClientId);
            e.HasIndex(x => new { x.ClientId, x.ViolationType });
        });

        modelBuilder.Entity<ClientBlock>(e =>
        {
            e.ToTable("client_blocks");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ClientId);
            e.HasIndex(x => new { x.ClientId, x.IsActive });
        });
    }
}
