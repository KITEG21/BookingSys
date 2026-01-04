using System;
using Reservation.Application.Interfaces;

namespace Reservation.Application.Queries.GetById;

public class GetReservationQueryHandler
{
    private readonly IReservationRepository _repo;
    public GetReservationQueryHandler(IReservationRepository repo) => _repo = repo;

    public async Task<Domain.Entities.Reservation?> Handle(GetReservationQuery query)
        => await _repo.GetAsync(query.Id);
}