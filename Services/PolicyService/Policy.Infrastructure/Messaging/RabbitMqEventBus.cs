using System.Text;
using System.Text.Json;
using Policy.Application.Interfaces;
using RabbitMQ.Client;

namespace Policy.Infrastructure.Messaging;

public class RabbitMqEventBus : IEventBus
{
    private readonly IConnection _connection;
    private IChannel? _channel;
    private const string ExchangeName = "events";

    public RabbitMqEventBus(IConnection connection)
    {
        _connection = connection;
    }

    public async Task PublishAsync<T>(T @event) where T : class
    {
        _channel ??= await _connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct, durable: true);

        var eventName = typeof(T).Name;
        var json = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(json);

        var props = new BasicProperties
        {
            DeliveryMode = DeliveryModes.Persistent
        };

        await _channel.BasicPublishAsync(ExchangeName, eventName, false, props, body);
    }
}
