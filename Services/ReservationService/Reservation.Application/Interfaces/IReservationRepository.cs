using System;

namespace Reservation.Application.Interfaces;

public interface IReservationRepository
{
    public Task AddAsync(Domain.Entities.Reservation reservation);
    public Task<Domain.Entities.Reservation?> GetAsync(Guid id);
    public Task UpdateAsync(Domain.Entities.Reservation reservation);
    public Task<IEnumerable<Domain.Entities.Reservation>> GetAllAsync();

}
