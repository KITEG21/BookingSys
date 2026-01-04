using Policy.Domain.Entities;

namespace Policy.Application.Interfaces;

public interface IViolationRepository
{
    Task<IEnumerable<ClientViolation>> GetByClientIdAsync(Guid clientId);
    Task<int> CountByClientIdAndTypeAsync(Guid clientId, string violationType);
    Task AddAsync(ClientViolation violation);
}
