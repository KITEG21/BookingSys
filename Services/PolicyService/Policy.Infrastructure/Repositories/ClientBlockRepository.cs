using Microsoft.EntityFrameworkCore;
using Policy.Application.Interfaces;
using Policy.Domain.Entities;
using Policy.Infrastructure.Persistence;

namespace Policy.Infrastructure.Repositories;

public class ClientBlockRepository : IClientBlockRepository
{
    private readonly PolicyDbContext _context;

    public ClientBlockRepository(PolicyDbContext context)
    {
        _context = context;
    }

    public async Task<ClientBlock?> GetActiveBlockAsync(Guid clientId)
    {
        return await _context.Blocks
            .Where(b => b.ClientId == clientId && b.IsActive)
            .Where(b => b.ExpiresAt == null || b.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> IsClientBlockedAsync(Guid clientId)
    {
        return await _context.Blocks
            .AnyAsync(b => b.ClientId == clientId 
                && b.IsActive 
                && (b.ExpiresAt == null || b.ExpiresAt > DateTime.UtcNow));
    }

    public async Task AddAsync(ClientBlock block)
    {
        _context.Blocks.Add(block);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ClientBlock block)
    {
        _context.Blocks.Update(block);
        await _context.SaveChangesAsync();
    }
}
