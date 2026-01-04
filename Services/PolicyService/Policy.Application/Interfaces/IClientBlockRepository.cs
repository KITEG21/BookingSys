using Policy.Domain.Entities;

namespace Policy.Application.Interfaces;

public interface IClientBlockRepository
{
    Task<ClientBlock?> GetActiveBlockAsync(Guid clientId);
    Task<bool> IsClientBlockedAsync(Guid clientId);
    Task AddAsync(ClientBlock block);
    Task UpdateAsync(ClientBlock block);
}
