using Audit.Api.Extensions;
using Audit.Infrastructure.Messaging;
using Audit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("ServiceName", "Audit.Api")
    .WriteTo.Console()
    .WriteTo.Seq(builder.Configuration["Seq:Url"] ?? "http://seq:5341")
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting {ServiceName}", "Audit.Api");

    builder.Services.AddControllers();
    builder.Services.AddAuditServices(builder.Configuration);

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Audit Service API",
            Version = "v1",
            Description = "Manages audit logs"
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
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Audit Service API v1");
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed for {ServiceName}", "Audit.Api");
}
finally
{
    Log.CloseAndFlush();
}
