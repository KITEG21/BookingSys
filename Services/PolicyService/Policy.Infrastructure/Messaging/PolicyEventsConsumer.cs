using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Policy.Application.Services;
using Shared.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Policy.Infrastructure.Messaging;

public class PolicyEventsConsumer
{
    private const string ExchangeName = "events";
    private readonly IConnection _connection;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PolicyEventsConsumer> _logger;
    private IChannel? _channel;

    public PolicyEventsConsumer(
        IConnection connection,
        IServiceScopeFactory scopeFactory,
        ILogger<PolicyEventsConsumer> logger)
    {
        _connection = connection;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync()
    {
        _channel = await _connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct, durable: true);

        // Queue for NoShow events
        await _channel.QueueDeclareAsync("PolicyNoShowQueue", durable: true, exclusive: false, autoDelete: false);
        await _channel.QueueBindAsync("PolicyNoShowQueue", ExchangeName, "NoShowReported");

        // Queue for Cancellation events
        await _channel.QueueDeclareAsync("PolicyCancellationQueue", durable: true, exclusive: false, autoDelete: false);
        await _channel.QueueBindAsync("PolicyCancellationQueue", ExchangeName, "ReservationCancelled");

        var noShowConsumer = new AsyncEventingBasicConsumer(_channel);
        noShowConsumer.ReceivedAsync += HandleNoShowAsync;
        await _channel.BasicConsumeAsync("PolicyNoShowQueue", autoAck: false, consumer: noShowConsumer);

        var cancellationConsumer = new AsyncEventingBasicConsumer(_channel);
        cancellationConsumer.ReceivedAsync += HandleCancellationAsync;
        await _channel.BasicConsumeAsync("PolicyCancellationQueue", autoAck: false, consumer: cancellationConsumer);

        _logger.LogInformation("PolicyEventsConsumer started, listening for NoShow and Cancellation events");
    }

    private async Task HandleNoShowAsync(object sender, BasicDeliverEventArgs ea)
    {
        try
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var evt = JsonSerializer.Deserialize<NoShowReported>(json);

            if (evt is null)
            {
                await _channel!.BasicNackAsync(ea.DeliveryTag, false, false);
                return;
            }

            _logger.LogInformation("Received NoShowReported for client {ClientId}", evt.ClientId);

            using var scope = _scopeFactory.CreateScope();
            var policyService = scope.ServiceProvider.GetRequiredService<PolicyEnforcementService>();
            await policyService.HandleNoShowAsync(evt.ClientId, evt.ReservationId);

            await _channel!.BasicAckAsync(ea.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing NoShowReported event");
            await _channel!.BasicNackAsync(ea.DeliveryTag, false, true);
        }
    }

    private async Task HandleCancellationAsync(object sender, BasicDeliverEventArgs ea)
    {
        try
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var evt = JsonSerializer.Deserialize<ReservationCancelled>(json);

            if (evt is null)
            {
                await _channel!.BasicNackAsync(ea.DeliveryTag, false, false);
                return;
            }

            _logger.LogInformation("Received ReservationCancelled for reservation {ReservationId}", evt.ReservationId);

            // For now, we treat all cancellations as potential late cancellations
            // In a real system, you'd check the reservation time vs current time
            using var scope = _scopeFactory.CreateScope();
            var policyService = scope.ServiceProvider.GetRequiredService<PolicyEnforcementService>();
            
            // TODO: Get clientId from reservation - for now we'll need to enhance this
            // await policyService.HandleCancellationAsync(clientId, evt.ReservationId, isLateCancellation: true);

            await _channel!.BasicAckAsync(ea.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ReservationCancelled event");
            await _channel!.BasicNackAsync(ea.DeliveryTag, false, true);
        }
    }
}
