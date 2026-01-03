using System;
using Reservation.Domain.Saga;

namespace Reservation.Application.Interfaces;

public interface ISagaRepository
{
    Task CreateAsync(ReservationSaga saga);
    Task<ReservationSaga?> GetAsync(Guid reservationId);
    Task UpdateAsync(ReservationSaga saga);
}
