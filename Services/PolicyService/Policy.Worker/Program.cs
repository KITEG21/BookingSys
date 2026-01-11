
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Policy.Infrastructure.Messaging;
using Policy.Infrastructure.Persistence;
using Policy.Worker.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", "Policy.Worker")
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting {ServiceName}", "Policy.Worker");

    var host = Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureServices((context, services) =>
        {
            services.AddPolicyServices(context.Configuration);
        })
        .Build();

    // Run migrations
    using (var scope = host.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<PolicyDbContext>();
        db.Database.Migrate();
    }

    // Start event consumer
    var consumer = host.Services.GetRequiredService<PolicyEventsConsumer>();
    await consumer.StartAsync();

    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed for {ServiceName}", "Policy.Worker");
}
finally
{
    Log.CloseAndFlush();
}
