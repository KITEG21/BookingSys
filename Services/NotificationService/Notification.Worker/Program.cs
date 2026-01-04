using Microsoft.EntityFrameworkCore;
using Notification.Infrastructure.Messaging;
using Notification.Infrastructure.Persistence;
using Notification.Worker.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddNotificationServices(builder.Configuration);

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
