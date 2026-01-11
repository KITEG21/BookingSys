using Microsoft.OpenApi.Models;
using Payment.Api.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("ServiceName", "Payment.Api")
    .WriteTo.Console()  // Keep console for Docker logs
    .WriteTo.Seq(builder.Configuration["Seq:Url"] ?? "http://seq:5341")
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    


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
