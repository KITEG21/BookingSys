using Microsoft.EntityFrameworkCore;
using Reporting.Application.Interfaces;
using Reporting.Application.Queries;
using Reporting.Application.Services;
using Reporting.Infrastructure.Messaging;
using Reporting.Infrastructure.Persistence;
using Reporting.Infrastructure.Repositories;
using RabbitMQ.Client;

namespace Reporting.Api.Extensions;

public static class DependencyInjectionSetup
{
    public static IServiceCollection AddReportingServices(this IServiceCollection services, IConfiguration configuration)
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
        services.AddDbContext<ReportingDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IReservationSummaryRepository, ReservationSummaryRepository>();
        services.AddScoped<IDailyStatsRepository, DailyStatsRepository>();

        // Services
        services.AddScoped<ReportingProjectionService>(sp => 
            new ReportingProjectionService(
                sp.GetRequiredService<IReservationSummaryRepository>(),
                sp.GetRequiredService<IDailyStatsRepository>(),
                sp.GetRequiredService<ILogger<ReportingProjectionService>>()
            ));
        services.AddScoped<ReportQueries>();

        // Messaging
        services.AddSingleton<ReportingEventsConsumer>(sp => 
            new ReportingEventsConsumer(
                sp.GetRequiredService<IConnection>(),
                sp.GetRequiredService<IServiceScopeFactory>(),
                sp.GetRequiredService<ILogger<ReportingEventsConsumer>>()
            ));

        return services;
    }
}
