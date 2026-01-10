using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Shared.Messaging;
using Policy.Application.Interfaces;
using Policy.Application.Services;
using Policy.Infrastructure.Messaging;
using Policy.Infrastructure.Persistence;
using Policy.Infrastructure.Repositories;
using RabbitMQ.Client;

namespace Policy.Worker.Extensions;

public static class DependencyInjectionSetup
{
    public static IServiceCollection AddPolicyServices(this IServiceCollection services, IConfiguration configuration)
    {
        // RabbitMQ - lazy connection with retry
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
                        logger?.LogInformation("Connecting to RabbitMQ {Host}:{Port} (attempt {Attempt})", 
                            factory.HostName, factory.Port, attempts + 1);
                        return factory.CreateConnectionAsync().GetAwaiter().GetResult();
                    }
                    catch (Exception ex) when (++attempts <= maxAttempts)
                    {
                        logger?.LogWarning(ex, "RabbitMQ not ready (attempt {Attempt}/{Max}). Retrying in {Delay}ms...", 
                            attempts, maxAttempts, delayMs);
                        Thread.Sleep(delayMs * attempts);
                    }
                }
            });
        });

        services.AddSingleton<IConnection>(sp => sp.GetRequiredService<Lazy<IConnection>>().Value);

        // Database
        services.AddDbContext<PolicyDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IViolationRepository, ViolationRepository>();
        services.AddScoped<IClientBlockRepository, ClientBlockRepository>();

        // Services
        services.AddScoped<PolicyEnforcementService>();

        // Messaging
        services.AddSingleton<IEventBus, Shared.Messaging.RabbitMqEventBus>();
        services.AddSingleton<PolicyEventsConsumer>();

        return services;
    }
}
