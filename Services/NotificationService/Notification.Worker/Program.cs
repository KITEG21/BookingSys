using Microsoft.EntityFrameworkCore;
using Notification.Infrastructure.Messaging;
using Notification.Infrastructure.Persistence;
using Notification.Worker.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile("appsettings.Development.json", optional: true)
        .AddEnvironmentVariables()
        .Build())
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("ServiceName", "Notification.Worker")
    .WriteTo.Console()
    .WriteTo.Seq("http://seq:5341")
    .CreateLogger();

try
{
    Log.Information("Starting {ServiceName}", "Notification.Worker");

    var builder = Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureServices((hostContext, services) =>
        {
            services.AddNotificationServices(hostContext.Configuration);
        });

    var host = builder.Build();

    // Run migrations
    using (var scope = host.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        db.Database.Migrate();
    }

    // Start event consumer
    var consumer = host.Services.GetRequiredService<NotificationEventsConsumer>();
    await consumer.StartAsync();

    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed for {ServiceName}", "Notification.Worker");
}
finally
{
    Log.CloseAndFlush();
}
