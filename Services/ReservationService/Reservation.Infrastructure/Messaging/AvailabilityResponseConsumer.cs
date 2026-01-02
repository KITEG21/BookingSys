using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Reservation.Domain.Events;
using Reservation.Application.Saga;

namespace Reservation.Infrastructure.Messaging;

public class AvailabilityResponseConsumer
{
    private const string ExchangeName = "events";
    private const string QueueName = "AvailabilityResponseQueue";
    private const string LockedKey = "AvailabilityLocked";
    private const string RejectedKey = "AvailabilityRejected";

    private readonly IConnection _connection;
    private IChannel? _channel;
    private readonly ReservationSagaOrchestrator _handler;

    public AvailabilityResponseConsumer(IConnection connection, ReservationSagaOrchestrator handler)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _handler = handler;
    }

    public async Task Start()
    {
        _channel ??= await _connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct, durable: true);
        await _channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false);
        await _channel.QueueBindAsync(QueueName, ExchangeName, LockedKey);
        await _channel.QueueBindAsync(QueueName, ExchangeName, RejectedKey);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var routingKey = ea.RoutingKey;
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                if (routingKey == LockedKey)
                {
                    var evt = JsonSerializer.Deserialize<AvailabilityLocked>(json);
                    if (evt is null) { await _channel.BasicNackAsync(ea.DeliveryTag, false, false); return; }
                    await _handler.HandleAsync(evt);
                }
                else if (routingKey == RejectedKey)
                {
                    var evt = JsonSerializer.Deserialize<AvailabilityRejected>(json);
                    if (evt is null) { await _channel.BasicNackAsync(ea.DeliveryTag, false, false); return; }
                    await _handler.HandleAsync(evt);
                }

                await _channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception)
            {
                await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
        };

        await _channel.BasicConsumeAsync(QueueName, autoAck: false, consumer: consumer);
    }
}