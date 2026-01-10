using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Events;
using Reservation.Application.Saga;

namespace Reservation.Infrastructure.Messaging;

public class PaymentSettledConsumer
{
    private const string ExchangeName = "events";
    private const string QueueName = "PaymentSettledQueue";
    private const string RoutingKey = "PaymentSettled";

    private readonly IConnection _connection;
    private IChannel? _channel;
    private readonly IServiceScopeFactory _scopeFactory;

    public PaymentSettledConsumer(IConnection connection, IServiceScopeFactory scopeFactory)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    }

    public async Task Start()
    {
        _channel ??= await _connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct, durable: true);
        await _channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false);
        await _channel.QueueBindAsync(QueueName, ExchangeName, RoutingKey);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var evt = JsonSerializer.Deserialize<PaymentSettled>(json);

                if (evt is null)
                {
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var orchestrator = scope.ServiceProvider.GetRequiredService<ReservationSagaOrchestrator>();
                await orchestrator.HandleAsync(evt);

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
