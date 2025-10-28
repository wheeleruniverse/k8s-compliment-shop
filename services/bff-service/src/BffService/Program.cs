using BffService.GraphQL.Queries;
using BffService.Services;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to use HTTP/1.1 for GraphQL and health checks
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8082, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });
});

// Get product service URL from environment variable or use default
var productServiceUrl = builder.Configuration.GetValue<string>("ProductService:Url") ?? "http://localhost:8081";

// Configure gRPC client for product-service
builder.Services.AddSingleton(services =>
{
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("GrpcClientFactory");

    logger.LogInformation("Configuring gRPC client for product-service at: {Url}", productServiceUrl);

    var channel = GrpcChannel.ForAddress(productServiceUrl, new GrpcChannelOptions
    {
        // Configure retry policy for transient failures
        MaxRetryAttempts = 3,
        // Log gRPC calls for debugging
        LoggerFactory = loggerFactory
    });

    return new ProductService.Protos.ProductService.ProductServiceClient(channel);
});

// Register our product service client wrapper
builder.Services.AddSingleton<IProductServiceClient, ProductServiceClient>();

// Configure GraphQL server with HotChocolate
builder.Services
    .AddGraphQLServer()
    .AddQueryType<ProductQueries>()
    .AddTypeExtension<ProductFieldResolvers>()
    // Enable GraphQL Playground in development
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = builder.Environment.IsDevelopment());

// Configure CORS for web frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()  // In production, specify exact origins
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Add health checks
builder.Services.AddHealthChecks();

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Enable CORS
app.UseCors();

// Map GraphQL endpoint
app.MapGraphQL("/graphql");

// Map health check endpoint
app.MapHealthChecks("/health");

// In development, show GraphQL Playground at root
if (app.Environment.IsDevelopment())
{
    app.Logger.LogInformation("GraphQL Playground available at: http://localhost:8082/graphql");
}

app.Logger.LogInformation("BFF Service starting...");
app.Logger.LogInformation("Product Service URL: {ProductServiceUrl}", productServiceUrl);
app.Logger.LogInformation("GraphQL endpoint: http://localhost:8082/graphql");
app.Logger.LogInformation("Health check endpoint: http://localhost:8082/health");

app.Run();

// Make Program class accessible for testing
public partial class Program { }
