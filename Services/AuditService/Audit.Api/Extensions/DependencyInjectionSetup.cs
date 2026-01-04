using Audit.Application.Interfaces;
using Audit.Application.Services;
using Audit.Infrastructure.Messaging;
using Audit.Infrastructure.Persistence;
using Audit.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

namespace Audit.Api.Extensions;

public static class DependencyInjectionSetup
{
    public static IServiceCollection AddAuditServices(this IServiceCollection services, IConfiguration configuration)
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
        services.AddDbContext<AuditDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Repository
        services.AddScoped<IAuditRepository, AuditRepository>();

        // Services
        services.AddScoped<AuditService>();

        // Messaging
        services.AddSingleton<AuditEventsConsumer>();

        return services;
    }
}
