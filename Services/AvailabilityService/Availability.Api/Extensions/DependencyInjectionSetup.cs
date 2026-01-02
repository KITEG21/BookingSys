using Availability.Application.Interfaces;
using Availability.Application.Services;
using Availability.Infrastructure.Messaging;
using RabbitMQ.Client;

namespace Availability.Api.ServicesExtensions;

public static class DependencyInjectionSetup
{
    public static IServiceCollection AddDependencyInjectionSetup(this IServiceCollection services,
        IConfiguration configuration)
    {
        // RabbitMQ connection
        services.AddSingleton<IConnection>(sp =>
        {
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
                UserName = configuration["RabbitMQ:UserName"] ?? "guest",
                Password = configuration["RabbitMQ:Password"] ?? "guest",
                Port = int.TryParse(configuration["RabbitMQ:Port"], out var port) ? port : 5672
            };
            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        });

        // Event bus
        services.AddSingleton<IEventBus, RabbitMqEventBus>();

        // Application services
        services.AddSingleton<AvailabilityService>();

        // Consumer
        services.AddSingleton<ReservationRequestedConsumer>();

        return services;
    }
}