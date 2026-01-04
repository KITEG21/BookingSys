using Microsoft.EntityFrameworkCore;
using Policy.Application.Interfaces;
using Policy.Domain.Entities;
using Policy.Infrastructure.Persistence;

namespace Policy.Infrastructure.Repositories;

public class ViolationRepository : IViolationRepository
{
    private readonly PolicyDbContext _context;

    public ViolationRepository(PolicyDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ClientViolation>> GetByClientIdAsync(Guid clientId)
    {
        return await _context.Violations
            .Where(v => v.ClientId == clientId)
            .OrderByDescending(v => v.OccurredAt)
            .ToListAsync();
    }

    public async Task<int> CountByClientIdAndTypeAsync(Guid clientId, string violationType)
    {
        return await _context.Violations
            .CountAsync(v => v.ClientId == clientId && v.ViolationType == violationType);
    }

    public async Task AddAsync(ClientViolation violation)
    {
        _context.Violations.Add(violation);
        await _context.SaveChangesAsync();
    }
}
