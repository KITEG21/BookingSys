using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Notification.Infrastructure.Messaging;

public class NotificationEventsConsumer
{
    private const string ExchangeName = "events";
    private readonly IConnection _connection;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationEventsConsumer> _logger;
    private IChannel? _channel;

    public NotificationEventsConsumer(
        IConnection connection,
        IServiceScopeFactory scopeFactory,
        ILogger<NotificationEventsConsumer> logger)
    {
        _connection = connection;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync()
    {
        _channel = await _connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct, durable: true);

        // Create queue for notifications
        await _channel.QueueDeclareAsync("NotificationQueue", durable: true, exclusive: false, autoDelete: false);

        // Bind to all events we care about
        await _channel.QueueBindAsync("NotificationQueue", ExchangeName, "ReservationConfirmed");
        await _channel.QueueBindAsync("NotificationQueue", ExchangeName, "ReservationCancelled");
        await _channel.QueueBindAsync("NotificationQueue", ExchangeName, "PaymentSettled");
        await _channel.QueueBindAsync("NotificationQueue", ExchangeName, "ReservationCompleted");
        await _channel.QueueBindAsync("NotificationQueue", ExchangeName, "ClientBlocked");

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += HandleEventAsync;
        await _channel.BasicConsumeAsync("NotificationQueue", autoAck: false, consumer: consumer);

        _logger.LogInformation("NotificationEventsConsumer started, listening for events");
    }

    private async Task HandleEventAsync(object sender, BasicDeliverEventArgs ea)
    {
        try
        {
            var eventType = ea.RoutingKey;
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());

            _logger.LogInformation("Received {EventType} event", eventType);

            using var scope = _scopeFactory.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<Application.Services.NotificationService>();

            switch (eventType)
            {
                case "ReservationConfirmed":
                    var confirmed = JsonSerializer.Deserialize<ReservationConfirmed>(json);
                    if (confirmed != null)
                        await notificationService.NotifyReservationConfirmedAsync(confirmed.ReservationId, confirmed.ClientEmail);
                    break;

                case "ReservationCancelled":
                    var cancelled = JsonSerializer.Deserialize<ReservationCancelled>(json);
                    if (cancelled != null)
                        await notificationService.NotifyReservationCancelledAsync(cancelled.ReservationId, cancelled.ClientEmail);
                    break;

                case "PaymentSettled":
                    var payment = JsonSerializer.Deserialize<PaymentSettled>(json);
                    if (payment != null)
                        await notificationService.NotifyPaymentSettledAsync(payment.ReservationId, payment.PaymentId, "client@example.com");
                    break;

                case "ReservationCompleted":
                    var completed = JsonSerializer.Deserialize<ReservationCompleted>(json);
                    if (completed != null)
                        await notificationService.NotifyReservationCompletedAsync(completed.ReservationId, completed.ClientEmail);
                    break;

                case "ClientBlocked":
                    var blocked = JsonSerializer.Deserialize<ClientBlocked>(json);
                    if (blocked != null)
                        await notificationService.NotifyClientBlockedAsync(blocked.ClientId, "client@example.com", blocked.Reason);
                    break;
            }

            await _channel!.BasicAckAsync(ea.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing notification event");
            await _channel!.BasicNackAsync(ea.DeliveryTag, false, true);
        }
    }
}
