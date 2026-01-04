using System;
using Reservation.Application.Interfaces;

namespace Reservation.Application.Queries.GetAll;

public class GetAllReservationsQueryHandler
{
private readonly IReservationRepository _repo;
    public GetAllReservationsQueryHandler(IReservationRepository repo) => _repo = repo;

    public async Task<IEnumerable<Domain.Entities.Reservation>> Handle(GetAllReservationsQuery query)
    {
        return await _repo.GetAllAsync();
    }

}
