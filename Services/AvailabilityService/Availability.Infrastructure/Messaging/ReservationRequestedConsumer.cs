using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Availability.Domain.Events;
using Availability.Application.Services;
using System.Threading.Tasks;

namespace Availability.Infrastructure.Messaging;

public class ReservationRequestedConsumer
{
    private const string ExchangeName = "events";
    private const string QueueName = "ReservationRequestedQueue";
    private const string RoutingKey = "ReservationRequested";

    private readonly IConnection _connection;
    private IChannel? _channel;
    private readonly AvailabilityService _availabilityService;

    public ReservationRequestedConsumer(IConnection connection, AvailabilityService availabilityService)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _availabilityService = availabilityService;
    }

    public async Task Start()
    {
        _channel ??= await _connection.CreateChannelAsync();

        // Exchange & queue setup
        await _channel.ExchangeDeclareAsync(exchange: ExchangeName, type: ExchangeType.Direct, durable: true);
        await _channel.QueueDeclareAsync(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        await _channel.QueueBindAsync(queue: QueueName, exchange: ExchangeName, routingKey: RoutingKey);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (sender, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var message = JsonSerializer.Deserialize<ReservationRequested>(json);

                if (message is null)
                {
                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                    return;
                }

                await _availabilityService.HandleAsync(message);
                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
            catch (Exception)
            {
                // On error requeue (or set to false per your policy)
                await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        await _channel.BasicConsumeAsync(queue: QueueName, autoAck: false, consumer: consumer);
    }
}