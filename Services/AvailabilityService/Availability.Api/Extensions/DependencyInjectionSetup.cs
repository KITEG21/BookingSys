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
        services.AddSingleton<IConnection>(sp =>
        {
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
                UserName = configuration["RabbitMQ:UserName"] ?? "guest",
                Password = configuration["RabbitMQ:Password"] ?? "guest",
                Port = int.TryParse(configuration["RabbitMQ:Port"], out var port) ? port : 5672
            };

            var attempts = 0;
            var maxAttempts = 10;
            var delayMs = 2000;
            while (true) try
                {
                    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
                }
                catch (Exception ex) when (++attempts <= maxAttempts)
                {
                    // log here using a logger from sp.GetRequiredService<ILogger<...>> if available
                    Thread.Sleep(delayMs);
                    Console.WriteLine(ex.Message);
                }
        });

        services.AddSingleton<IEventBus, RabbitMqEventBus>();

        // Application services
        services.AddSingleton<AvailabilityService>();

        services.AddSingleton<ReservationRequestedConsumer>();





        return services;
    }
}