using Audit.Api.Extensions;
using Audit.Infrastructure.Messaging;
using Audit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddAuditServices(builder.Configuration);

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Reservation Service API",
        Version = "v1",
        Description = "Manages reservations and sagas"
    });
    // Optional: Add JWT security definition for auth-required endpoints
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter JWT token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
});

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

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Reservation Service API v1");
});

app.Run();
