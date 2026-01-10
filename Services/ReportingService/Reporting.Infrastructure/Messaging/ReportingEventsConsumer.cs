using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Reporting.Application.Services;
using Shared.Events;

namespace Reporting.Infrastructure.Messaging;

public class ReportingEventsConsumer
{
    private const string ExchangeName = "events";
    private readonly IConnection _connection;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReportingEventsConsumer> _logger;
    private IChannel? _channel;

    public ReportingEventsConsumer(
        IConnection connection,
        IServiceScopeFactory scopeFactory,
        ILogger<ReportingEventsConsumer> logger)
    {
        _connection = connection;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync()
    {
        _channel = await _connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct, durable: true);

        // Create queue for reporting projections
        await _channel.QueueDeclareAsync("ReportingQueue", durable: true, exclusive: false, autoDelete: false);

        // Bind to all reservation events
        await _channel.QueueBindAsync("ReportingQueue", ExchangeName, "ReservationRequested");
        await _channel.QueueBindAsync("ReportingQueue", ExchangeName, "ReservationConfirmed");
        await _channel.QueueBindAsync("ReportingQueue", ExchangeName, "ReservationCancelled");
        await _channel.QueueBindAsync("ReportingQueue", ExchangeName, "ReservationCompleted");
        await _channel.QueueBindAsync("ReportingQueue", ExchangeName, "PaymentSettled");

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += HandleEventAsync;
        await _channel.BasicConsumeAsync("ReportingQueue", autoAck: false, consumer: consumer);

        _logger.LogInformation("ReportingEventsConsumer started, listening for events");
    }

    private async Task HandleEventAsync(object sender, BasicDeliverEventArgs ea)
    {
        var shouldAck = true;
        
        try
        {
            var eventType = ea.RoutingKey;
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());

            _logger.LogInformation("Received {EventType} event for projection", eventType);

            using var scope = _scopeFactory.CreateScope();
            var projectionService = scope.ServiceProvider.GetRequiredService<ReportingProjectionService>();

            switch (eventType)
            {
                case "ReservationRequested":
                    var requested = JsonSerializer.Deserialize<ReservationRequested>(json);
                    if (requested != null)
                        await projectionService.HandleReservationRequestedAsync(requested);
                    break;

                case "ReservationConfirmed":
                    var confirmed = JsonSerializer.Deserialize<ReservationConfirmed>(json);
                    if (confirmed != null)
                        await projectionService.HandleReservationConfirmedAsync(confirmed);
                    break;

                case "ReservationCancelled":
                    var cancelled = JsonSerializer.Deserialize<ReservationCancelled>(json);
                    if (cancelled != null)
                        await projectionService.HandleReservationCancelledAsync(cancelled);
                    break;

                case "ReservationCompleted":
                    var completed = JsonSerializer.Deserialize<ReservationCompleted>(json);
                    if (completed != null)
                        await projectionService.HandleReservationCompletedAsync(completed);
                    break;

                case "PaymentSettled":
                    var payment = JsonSerializer.Deserialize<PaymentSettled>(json);
                    if (payment != null)
                        await projectionService.HandlePaymentSettledAsync(payment);
                    break;
            }

            _logger.LogInformation("Successfully processed {EventType} event", eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing reporting event");
            
            // ACK anyway to prevent infinite loop - projections are idempotent now
            // In production, consider dead-letter queue for persistent failures
            shouldAck = true;
        }
        finally
        {
            if (shouldAck)
            {
                await _channel!.BasicAckAsync(ea.DeliveryTag, false);
            }
        }
    }
} 