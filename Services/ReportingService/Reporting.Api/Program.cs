using Microsoft.EntityFrameworkCore;
using Reporting.Api.Extensions;
using Reporting.Infrastructure.Messaging;
using Reporting.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddReportingServices(builder.Configuration);

var app = builder.Build();

// Run migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ReportingDbContext>();
    db.Database.Migrate();
}

// Start event consumer
var consumer = app.Services.GetRequiredService<ReportingEventsConsumer>();
_ = consumer.StartAsync();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
