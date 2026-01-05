using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Repositories;
using ProductService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire ServiceDefaults (health checks, telemetry, service discovery)
builder.AddServiceDefaults();

// Configure Kestrel with separate endpoints for HTTP/1.1 and HTTP/2
builder.WebHost.ConfigureKestrel(options =>
{
    // HTTP/1.1 endpoint for health checks and web requests
    options.ListenAnyIP(8080, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
    });

    // HTTP/2 endpoint for gRPC
    options.ListenAnyIP(8081, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

// Add services to the container
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

// Configure MySQL with Aspire integration
builder.AddMySqlDbContext<ProductDbContext>("complimentshop");

// Register repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();

var app = builder.Build();

// Ensure database is created and seeded with retry logic and exponential backoff
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    var maxRetries = 30;
    var initialDelay = TimeSpan.FromSeconds(2);
    var maxDelay = TimeSpan.FromSeconds(10);

    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            app.Logger.LogInformation("Attempting to connect to database (attempt {Attempt}/{MaxRetries})...", i + 1, maxRetries);
            dbContext.Database.EnsureCreated();
            app.Logger.LogInformation("Database created and seeded successfully");
            break;
        }
        catch (Exception ex) when (i < maxRetries - 1)
        {
            // Exponential backoff: delay doubles each retry, capped at maxDelay
            var delay = TimeSpan.FromMilliseconds(Math.Min(initialDelay.TotalMilliseconds * Math.Pow(2, i), maxDelay.TotalMilliseconds));
            app.Logger.LogWarning(ex, "Failed to connect to database. Retrying in {Delay:F1} seconds...", delay.TotalSeconds);
            await Task.Delay(delay);
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Failed to create database after {MaxRetries} attempts", maxRetries);
            throw;
        }
    }
}

// Configure the HTTP request pipeline
app.MapGrpcService<ProductGrpcService>();
app.MapGrpcReflectionService();

// Map Aspire health endpoints
app.MapDefaultEndpoints();

app.MapGet("/", () => "Product Service - gRPC communication only. Use a gRPC client to interact with this service.");

app.Run();
