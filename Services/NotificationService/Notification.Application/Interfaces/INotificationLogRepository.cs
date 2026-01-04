using Notification.Domain.Entities;

namespace Notification.Application.Interfaces;

public interface INotificationLogRepository
{
    Task AddAsync(NotificationLog log);
    Task<IEnumerable<NotificationLog>> GetRecentAsync(int count = 100);
}
