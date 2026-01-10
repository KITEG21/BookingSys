using Microsoft.Extensions.Logging;
using Shared.Interfaces;
using Shared.Events;
using Policy.Application.Interfaces;
using Policy.Domain.Entities;

namespace Policy.Application.Services;

public class PolicyEnforcementService
{
    private readonly IViolationRepository _violationRepository;
    private readonly IClientBlockRepository _blockRepository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<PolicyEnforcementService> _logger;

    private const int MaxNoShowsBeforeBlock = 3;
    private const int MaxCancellationsBeforeWarning = 5;

    public PolicyEnforcementService(
        IViolationRepository violationRepository,
        IClientBlockRepository blockRepository,
        IEventBus eventBus,
        ILogger<PolicyEnforcementService> logger)
    {
        _violationRepository = violationRepository;
        _blockRepository = blockRepository;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task HandleNoShowAsync(Guid clientId, Guid reservationId)
    {
        _logger.LogInformation("Processing no-show for client {ClientId}, reservation {ReservationId}", 
            clientId, reservationId);

        // Record the violation
        var violation = new ClientViolation(clientId, "NoShow", reservationId);
        await _violationRepository.AddAsync(violation);

        // Count no-shows
        var noShowCount = await _violationRepository.CountByClientIdAndTypeAsync(clientId, "NoShow");

        // Apply penalty
        await _eventBus.PublishAsync(new PenaltyApplied(
            clientId, 
            reservationId, 
            "NoShow", 
            $"No-show #{noShowCount} recorded"));

        // Check if client should be blocked
        if (noShowCount >= MaxNoShowsBeforeBlock)
        {
            var existingBlock = await _blockRepository.GetActiveBlockAsync(clientId);
            if (existingBlock is null)
            {
                var block = new ClientBlock(
                    clientId, 
                    $"Blocked due to {noShowCount} no-shows",
                    DateTime.UtcNow.AddDays(30)); // 30-day block

                await _blockRepository.AddAsync(block);

                await _eventBus.PublishAsync(new ClientBlocked(
                    clientId, 
                    block.Reason, 
                    block.BlockedAt));

                _logger.LogWarning("Client {ClientId} has been blocked due to {Count} no-shows", 
                    clientId, noShowCount);
            }
        }
    }

    public async Task HandleCancellationAsync(Guid clientId, Guid reservationId, bool isLateCancellation)
    {
        _logger.LogInformation("Processing cancellation for client {ClientId}, reservation {ReservationId}, late: {IsLate}", 
            clientId, reservationId, isLateCancellation);

        if (isLateCancellation)
        {
            var violation = new ClientViolation(clientId, "LateCancellation", reservationId);
            await _violationRepository.AddAsync(violation);

            var cancellationCount = await _violationRepository.CountByClientIdAndTypeAsync(clientId, "LateCancellation");

            await _eventBus.PublishAsync(new PenaltyApplied(
                clientId, 
                reservationId, 
                "LateCancellation", 
                $"Late cancellation #{cancellationCount} recorded"));

            if (cancellationCount >= MaxCancellationsBeforeWarning)
            {
                _logger.LogWarning("Client {ClientId} has {Count} late cancellations - warning threshold reached", 
                    clientId, cancellationCount);
            }
        }
    }

    public async Task<bool> CanClientMakeReservationAsync(Guid clientId)
    {
        return !await _blockRepository.IsClientBlockedAsync(clientId);
    }
}
