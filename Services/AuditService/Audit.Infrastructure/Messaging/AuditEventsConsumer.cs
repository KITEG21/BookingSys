using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Audit.Infrastructure.Messaging;

public class AuditEventsConsumer
{
    private const string ExchangeName = "events";
    private readonly IConnection _connection;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditEventsConsumer> _logger;
    private IChannel? _channel;

    public AuditEventsConsumer(
        IConnection connection,
        IServiceScopeFactory scopeFactory,
        ILogger<AuditEventsConsumer> logger)
    {
        _connection = connection;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync()
    {
        _channel = await _connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct, durable: true);

        // Create queue for audit - listens to ALL events
        await _channel.QueueDeclareAsync("AuditQueue", durable: true, exclusive: false, autoDelete: false);

        // Bind to all events we want to audit
        var eventTypes = new[]
        {
            "ReservationRequested",
            "ReservationConfirmed",
            "ReservationCancelled",
            "ReservationCompleted",
            "AvailabilityLocked",
            "AvailabilityRejected",
            "PaymentSettled",
            "ClientBlocked",
            "PenaltyApplied",
            "NoShowReported"
        };

        foreach (var eventType in eventTypes)
        {
            await _channel.QueueBindAsync("AuditQueue", ExchangeName, eventType);
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += HandleEventAsync;
        await _channel.BasicConsumeAsync("AuditQueue", autoAck: false, consumer: consumer);

        _logger.LogInformation("AuditEventsConsumer started, listening to {Count} event types", eventTypes.Length);
    }

    private async Task HandleEventAsync(object sender, BasicDeliverEventArgs ea)
    {
        try
        {
            var eventType = ea.RoutingKey;
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());

            _logger.LogDebug("Auditing event: {EventType}", eventType);

            // Parse JSON to extract entity info
            Guid? entityId = null;
            string? entityType = null;

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Try common patterns for entity IDs
                if (root.TryGetProperty("ReservationId", out var reservationId))
                {
                    entityId = reservationId.GetGuid();
                    entityType = "Reservation";
                }
                else if (root.TryGetProperty("ClientId", out var clientId))
                {
                    entityId = clientId.GetGuid();
                    entityType = "Client";
                }
                else if (root.TryGetProperty("PaymentId", out var paymentId))
                {
                    entityId = paymentId.GetGuid();
                    entityType = "Payment";
                }
            }
            catch { /* Ignore parsing errors */ }

            using var scope = _scopeFactory.CreateScope();
            var auditService = scope.ServiceProvider.GetRequiredService<Application.Services.AuditService>();

            await auditService.RecordEventAsync(
                eventType,
                json,
                entityId,
                entityType,
                actor: null,
                sourceService: "EventBus",
                correlationId: null);

            await _channel!.BasicAckAsync(ea.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing audit event");
            await _channel!.BasicNackAsync(ea.DeliveryTag, false, true);
        }
    }
}
