using Gateway.Api.Persistence;
using Gateway.Api.Repositories;
using Gateway.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// === Database ===
builder.Services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// === Repositories & Services ===
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

// === Controllers ===
builder.Services.AddControllers();

// === JWT Authentication ===
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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
});

// === YARP Reverse Proxy ===
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// === CORS ===
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// === Health Check ===
builder.Services.AddHealthChecks();

// === Swagger ===

    // === Swagger (Development only) ===
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "BookingSys API Gateway",
            Version = "v1",
            Description = "API Gateway for BookingSys microservices"
        });
    });


var app = builder.Build();

// === Apply Migrations ===
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    db.Database.Migrate();
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();


    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "BookingSys API Gateway v1");
    });


app.MapControllers();
app.MapHealthChecks("/health");

// Map YARP proxy
app.MapReverseProxy();

app.Run();
