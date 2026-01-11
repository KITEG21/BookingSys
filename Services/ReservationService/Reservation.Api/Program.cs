using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using Reservation.Api.Services;
using Reservation.Api.ServicesExtensions;
using Reservation.Application.Interfaces;
using Reservation.Infrastructure.Messaging;
using Reservation.Infrastructure.Persistence;
using Serilog;


var builder = WebApplication.CreateBuilder(args);


Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("ServiceName", "Reservation.Api")
    .WriteTo.Console()  // Keep console for Docker logs
    .WriteTo.Seq(builder.Configuration["Seq:Url"] ?? "http://seq:5341")
    .CreateLogger();

builder.Host.UseSerilog();

try{
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

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

builder.Services.AddHttpClient<IUserContext, HttpContextUserContext>(client =>
{
    var gatewayUrl = builder.Configuration["Services:Gateway"] ?? "http://localhost:5000";
    client.BaseAddress = new Uri(gatewayUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddDependencyInjectionSetup(builder.Configuration); 


var jwtKey = builder.Configuration["Jwt:Key"] ?? "BookingSysSecretKey2025!MustBe32Chars";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "BookingSys";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ReservationDbContext>();
    db.Database.Migrate();
}

var availabilityConsumer = app.Services.GetRequiredService<AvailabilityResponseConsumer>();
_ = availabilityConsumer.Start();

var paymentConsumer = app.Services.GetRequiredService<PaymentSettledConsumer>();
_ = paymentConsumer.Start();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}
