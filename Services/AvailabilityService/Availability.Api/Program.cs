using Availability.Api.ServicesExtensions;
using Availability.Infrastructure.Messaging;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddDependencyInjectionSetup(builder.Configuration);

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

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Reservation Service API v1");
});

app.UseHttpsRedirection();
app.MapControllers();

// Start the consumer
var consumer = app.Services.GetRequiredService<ReservationRequestedConsumer>();
await consumer.Start();

app.Run();

