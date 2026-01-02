using Availability.Api.ServicesExtensions;
using Availability.Infrastructure.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddDependencyInjectionSetup(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

// Start the consumer
var consumer = app.Services.GetRequiredService<ReservationRequestedConsumer>();
consumer.Start();

app.Run();

