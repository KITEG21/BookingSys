using Availability.Application.Interfaces;
using Availability.Domain.Entities;
using Availability.Domain.Events;

namespace Availability.Application.Services;

public class AvailabilityService
{
    private static readonly List<TimeSlot> _lockedSlots = new();
    private static readonly object _lock = new();

    private readonly IEventBus _eventBus;

    public AvailabilityService(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task HandleAsync(ReservationRequested request)
    {
        var requestedSlot = new TimeSlot(request.Start, request.End);

        bool isAvailable;
        lock (_lock)
        {
            isAvailable = !_lockedSlots.Any(s => s.Overlaps(requestedSlot));
            if (isAvailable)
            {
                _lockedSlots.Add(requestedSlot);
            }
        }

        if (!isAvailable)
        {
            await _eventBus.PublishAsync(new AvailabilityRejected(
                request.ReservationId
            ));
            return;
        }

        await _eventBus.PublishAsync(new AvailabilityLocked(
            request.ReservationId,
            request.Start,
            request.End
        ));
    }
}
