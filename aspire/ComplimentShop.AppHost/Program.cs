var builder = DistributedApplication.CreateBuilder(args);

// MySQL password from environment variable (for GitHub Actions secrets) or default for local dev
var mysqlPassword = Environment.GetEnvironmentVariable("MYSQL_ROOT_PASSWORD") ?? "local_dev_password";

// Create Aspire parameter with the password value
// This ensures both MySQL container and health checks use the same password
var passwordParam = builder.AddParameter("mysql-password", secret: true);
builder.Configuration["Parameters:mysql-password"] = mysqlPassword;

// MySQL database resource (stops when Aspire stops)
// TODO: Re-enable persistent lifetime once initial setup is working
// TODO: Re-enable data volume once password management is stable (.WithDataVolume())
var mysql = builder.AddMySql("mysql", password: passwordParam);

var complimentshopDb = mysql.AddDatabase("complimentshop");

// ProductService - gRPC API (port 8081) + HTTP health checks (port 8080)
// Waits for MySQL to be healthy before starting
var productService = builder.AddProject<Projects.ProductService>("product-service")
    .WithReference(complimentshopDb)
    .WaitFor(complimentshopDb);

// BffService - GraphQL gateway, uses gRPC client to call ProductService
// Waits for ProductService to be healthy before starting
var bffService = builder.AddProject<Projects.BffService>("bff-service")
    .WithReference(productService)
    .WaitFor(productService);

// WebService - Blazor UI, uses GraphQL client to call BffService
// Waits for BffService to be healthy before starting
var webService = builder.AddProject<Projects.WebService>("web-service")
    .WithReference(bffService)
    .WaitFor(bffService)
    .WithExternalHttpEndpoints();

await builder.Build().RunAsync();
