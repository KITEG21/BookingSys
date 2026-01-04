using Microsoft.EntityFrameworkCore;
using Reservation.Application.Interfaces;
using Reservation.Domain.Entities;
using Reservation.Infrastructure.Persistence;

namespace Reservation.Infrastructure.Repositories;

public class EfReservationRepository : IReservationRepository
{
    private readonly ReservationDbContext _db;
    public EfReservationRepository(ReservationDbContext db) => _db = db;

    public async Task AddAsync(Domain.Entities.Reservation reservation)
    {
        _db.Reservations.Add(reservation);
        await _db.SaveChangesAsync();
    }

    public async Task<Domain.Entities.Reservation?> GetAsync(Guid id) =>
        await _db.Reservations.FindAsync(id);

    public async Task UpdateAsync(Domain.Entities.Reservation reservation)
    {
        _db.Reservations.Update(reservation);
        await _db.SaveChangesAsync();
    }
    public async Task<IEnumerable<Domain.Entities.Reservation>> GetAllAsync() =>
    await _db.Reservations.ToListAsync();
}