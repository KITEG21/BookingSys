

using RabbitMQ.Client;
using Reservation.Application.Commands.Post;
using Reservation.Infrastructure.Messaging;

namespace Reservation.Api.ServicesExtensions;

public static class DependencyInjectionSetup
{
    public static IServiceCollection AddDependencyInjectionSetup(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IConnection>(sp =>
        {
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
                UserName = configuration["RabbitMQ:UserName"] ?? "guest",
                Password = configuration["RabbitMQ:Password"] ?? "guest",
                Port = int.TryParse(configuration["RabbitMQ:Port"], out var port) ? port : 5672
            };
            // Use GetAwaiter().GetResult() to synchronously create the async connection in DI
            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        });

        services.AddTransient<CreateReservationCommandHandler>();

        // Register the event bus
        services.AddSingleton<IEventBus, RabbitMqEventBus>();




        return services;
    }
}