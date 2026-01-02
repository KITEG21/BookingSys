using System;
using System.Text;
using System.Text.Json;
using Availability.Application.Interfaces;
using RabbitMQ.Client;

namespace Availability.Infrastructure.Messaging;

public class RabbitMqEventBus : IEventBus, IDisposable
{
    private readonly IConnection _connection;
    private IChannel? _channel;
    private bool _disposed;

    public RabbitMqEventBus(IConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    public async Task PublishAsync<T>(T @event) where T : class
    {
        if (@event == null) throw new ArgumentNullException(nameof(@event));

        var channel = await GetChannelAsync();

        // Declare a direct exchange (create if it doesn't exist) for better event routing
        await channel.ExchangeDeclareAsync("events", ExchangeType.Direct, durable: true);

        var eventType = typeof(T).Name;

        // Declare a queue for the event type (create if it doesn't exist)
        await channel.QueueDeclareAsync(eventType, durable: true, exclusive: false, autoDelete: false);

        // Bind the queue to the exchange with the event type as routing key
        await channel.QueueBindAsync(eventType, "events", eventType);

        var message = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(message);

        // Publish with routing key as event type name
        await channel.BasicPublishAsync("events", eventType, body: body);
    }

    private async Task<IChannel> GetChannelAsync()
    {
        if (_channel == null || _channel.IsClosed)
        {
            _channel = await _connection.CreateChannelAsync();
        }
        return _channel;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing && _channel != null)
            {
                _channel.CloseAsync().GetAwaiter().GetResult();
            }
            _disposed = true;
        }
    }
}