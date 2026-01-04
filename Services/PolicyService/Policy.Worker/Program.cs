using Microsoft.EntityFrameworkCore;
using Policy.Infrastructure.Messaging;
using Policy.Infrastructure.Persistence;
using Policy.Worker.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddPolicyServices(builder.Configuration);

var host = builder.Build();

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
