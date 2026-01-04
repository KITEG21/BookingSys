using Reporting.Domain.ReadModels;

namespace Reporting.Application.Interfaces;

public interface IReservationSummaryRepository
{
    Task<ReservationSummary?> GetByReservationIdAsync(Guid reservationId);
    Task<IEnumerable<ReservationSummary>> GetAllAsync();
    Task<IEnumerable<ReservationSummary>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task AddAsync(ReservationSummary summary);
    Task UpdateAsync(ReservationSummary summary);
}
