using Microsoft.EntityFrameworkCore;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Infrastructure.Persistence;

namespace Notification.Infrastructure.Repositories;

public class NotificationLogRepository : INotificationLogRepository
{
    private readonly NotificationDbContext _context;

    public NotificationLogRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(NotificationLog log)
    {
        _context.NotificationLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<NotificationLog>> GetRecentAsync(int count = 100)
    {
        return await _context.NotificationLogs
            .OrderByDescending(x => x.SentAt)
            .Take(count)
            .ToListAsync();
    }
}
