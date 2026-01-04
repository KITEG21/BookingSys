using Audit.Api.Extensions;
using Audit.Infrastructure.Messaging;
using Audit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddAuditServices(builder.Configuration);

var app = builder.Build();

// Run migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
    db.Database.Migrate();
}

// Start event consumer
var consumer = app.Services.GetRequiredService<AuditEventsConsumer>();
_ = consumer.StartAsync();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
