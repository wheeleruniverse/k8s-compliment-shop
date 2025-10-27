using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Repositories;
using ProductService.Services;

var builder = WebApplication.CreateBuilder(args);

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

// Configure MySQL with Pomelo
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ProductDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// Register repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ProductDbContext>();

var app = builder.Build();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    try
    {
        dbContext.Database.EnsureCreated();
        app.Logger.LogInformation("Database created and seeded successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error creating database");
    }
}

// Configure the HTTP request pipeline
app.MapGrpcService<ProductGrpcService>();
app.MapGrpcReflectionService();

app.MapHealthChecks("/health");

app.MapGet("/", () => "Product Service - gRPC communication only. Use a gRPC client to interact with this service.");

app.Run();
