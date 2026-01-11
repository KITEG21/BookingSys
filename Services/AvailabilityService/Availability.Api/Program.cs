using Availability.Api.ServicesExtensions;
using Availability.Infrastructure.Messaging;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("ServiceName", "Availability.Api")
    .WriteTo.Console()
    .WriteTo.Seq(builder.Configuration["Seq:Url"] ?? "http://seq:5341")
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting {ServiceName}", "Availability.Api");

// Add services to the container
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddDependencyInjectionSetup(builder.Configuration);

// Add services to the container
    builder.Services.AddOpenApi();
    builder.Services.AddControllers();
    builder.Services.AddDependencyInjectionSetup(builder.Configuration);

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Availability Service API",
            Version = "v1",
            Description = "Manages availability"
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

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Availability Service API v1");
    });

    app.UseHttpsRedirection();
    app.MapControllers();

    // Start the consumer
    var consumer = app.Services.GetRequiredService<ReservationRequestedConsumer>();
    await consumer.Start();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed for {ServiceName}", "Availability.Api");
}
finally
{
    Log.CloseAndFlush();
}

