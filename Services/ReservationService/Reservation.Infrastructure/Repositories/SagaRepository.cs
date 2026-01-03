using System;
using Reservation.Application.Interfaces;
using Reservation.Domain.Saga;
using Reservation.Infrastructure.Persistence;

namespace Reservation.Infrastructure.Repositories;

public class SagaRepository : ISagaRepository
{
    private readonly ReservationDbContext _db;
    public SagaRepository(ReservationDbContext db) => _db = db;
    
    public async Task CreateAsync(ReservationSaga saga)
    {
        var entity = new ReservationSagaEntity
        {
            ReservationId = saga.ReservationId,
            State = saga.State.ToString(),
            UpdatedAt = DateTime.UtcNow
        };
        _db.ReservationSagas.Add(entity);
        await _db.SaveChangesAsync();
    }

    public async Task<ReservationSaga?> GetAsync(Guid reservationId)
    {
        var entity = await _db.ReservationSagas.FindAsync(reservationId);
        if (entity is null) return null;

        var saga = new ReservationSaga(entity.ReservationId);
        switch (entity.State)
        {
            case "WaitingForAvailability": saga.MarkWaitingForAvailability(); break;
            case "Confirmed": saga.Confirm(); break;
            case "Rejected": saga.Reject(); break;
            default: /* Started or unknown */ break;
        }
        return saga;
    }

    public async Task UpdateAsync(ReservationSaga saga)
    {
        var entity = await _db.ReservationSagas.FindAsync(saga.ReservationId);
        if (entity == null)
        {
            entity = new ReservationSagaEntity
            {
                ReservationId = saga.ReservationId,
                State = saga.State.ToString(),
                UpdatedAt = DateTime.UtcNow
            };
            _db.ReservationSagas.Add(entity);
        }
        else
        {
            entity.State = saga.State.ToString();
            entity.UpdatedAt = DateTime.UtcNow;
            _db.ReservationSagas.Update(entity);
        }
        await _db.SaveChangesAsync();
    }
}
