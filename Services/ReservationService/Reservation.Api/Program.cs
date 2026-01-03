
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using Reservation.Api.ServicesExtensions;
using Reservation.Infrastructure.Messaging;
using Reservation.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddDependencyInjectionSetup(builder.Configuration); 




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ReservationDbContext>();
    db.Database.Migrate();
}

var availabilityConsumer = app.Services.GetRequiredService<AvailabilityResponseConsumer>();
_ = availabilityConsumer.Start();

var paymentConsumer = app.Services.GetRequiredService<PaymentSettledConsumer>();
_ = paymentConsumer.Start();

app.MapControllers();

app.Run();

