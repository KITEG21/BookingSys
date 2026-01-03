using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using Reservation.Application.Commands.Post;
using Reservation.Application.Interfaces;
using Reservation.Application.Saga;
using Reservation.Infrastructure.Messaging;
using Reservation.Infrastructure.Persistence;
using Reservation.Infrastructure.Repositories;

namespace Reservation.Api.ServicesExtensions;

public static class DependencyInjectionSetup
{
    public static IServiceCollection AddDependencyInjectionSetup(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register Lazy<IConnection> to defer connection until first use
        services.AddSingleton<Lazy<IConnection>>(sp =>
        {
            return new Lazy<IConnection>(() =>
            {
                var cfg = sp.GetRequiredService<IConfiguration>();
                var logger = sp.GetService<ILoggerFactory>()?.CreateLogger("RabbitMQ");
                var factory = new ConnectionFactory
                {
                    HostName = cfg["RabbitMQ:HostName"] ?? "localhost",
                    UserName = cfg["RabbitMQ:UserName"] ?? "guest",
                    Password = cfg["RabbitMQ:Password"] ?? "guest",
                    Port = int.TryParse(cfg["RabbitMQ:Port"], out var p) ? p : 5672,
                };

                var attempts = 0;
                var maxAttempts = 20;
                var delayMs = 2000;

                while (true)
                {
                    try
                    {
                        logger?.LogInformation("Connecting to RabbitMQ {Host}:{Port} (attempt {Attempt})", factory.HostName, factory.Port, attempts + 1);
                        return factory.CreateConnectionAsync().GetAwaiter().GetResult();
                    }
                    catch (Exception ex) when (++attempts <= maxAttempts)
                    {
                        logger?.LogWarning(ex, "RabbitMQ not ready yet (attempt {Attempt}/{Max}). Retrying in {Delay}ms...", attempts, maxAttempts, delayMs);
                        Thread.Sleep(delayMs * attempts);
                    }
                }
            });
        });

        // Register IConnection that resolves the Lazy value
        services.AddSingleton<IConnection>(sp => sp.GetRequiredService<Lazy<IConnection>>().Value);

        services.AddTransient<CreateReservationCommandHandler>();

        services.AddDbContext<ReservationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IReservationRepository, EfReservationRepository>();
        services.AddScoped<ISagaRepository, SagaRepository>();
        services.AddScoped<ReservationSagaOrchestrator>();
        services.AddSingleton<AvailabilityResponseConsumer>();
        services.AddSingleton<IEventBus, RabbitMqEventBus>();

        return services;
    }
}