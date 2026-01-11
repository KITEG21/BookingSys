using Shared.Interfaces;
using Shared.Events;
using Availability.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Availability.Application.Services;

public class AvailabilityService
{
    private static readonly List<TimeSlot> _lockedSlots = new();
    private static readonly object _lock = new();

    private readonly IEventBus _eventBus;
    private readonly ILogger<AvailabilityService> _logger;

    public AvailabilityService(IEventBus eventBus, ILogger<AvailabilityService> logger)
    {
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task HandleAsync(ReservationRequested request)
    {
        _logger.LogInformation("Handling ReservationRequested for ReservationId {ReservationId}, Start {Start}, End {End}", 
            request.ReservationId, request.Start, request.End);

        var requestedSlot = new TimeSlot(request.Start, request.End);

        bool isAvailable;
        lock (_lock)
        {
            isAvailable = !_lockedSlots.Any(s => s.Overlaps(requestedSlot));
            if (isAvailable)
            {
                _lockedSlots.Add(requestedSlot);
                _logger.LogInformation("Slot locked for ReservationId {ReservationId}", request.ReservationId);
            }
            else
            {
                _logger.LogWarning("Slot not available for ReservationId {ReservationId}", request.ReservationId);
            }
        }

        if (!isAvailable)
        {
            await _eventBus.PublishAsync(new AvailabilityRejected(
                request.ReservationId
            ));
            _logger.LogInformation("Published AvailabilityRejected for ReservationId {ReservationId}", request.ReservationId);
            return;
        }

        await _eventBus.PublishAsync(new AvailabilityLocked(
            request.ReservationId,
            request.Start,
            request.End
        ));
        _logger.LogInformation("Published AvailabilityLocked for ReservationId {ReservationId}", request.ReservationId);
    }
}
