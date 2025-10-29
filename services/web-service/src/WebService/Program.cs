using WebService.Components;
using WebService.GraphQL;
using WebService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register analytics service
builder.Services.AddScoped<AnalyticsService>();

// Configure GraphQL client for BFF service
var bffServiceUrl = builder.Configuration["BffService:Url"] ?? "http://localhost:8082/graphql";
builder.Services
    .AddComplimentShopClient()
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri(bffServiceUrl);
    });

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Disable HTTPS redirection for K8s (handled by Ingress)
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseAntiforgery();

// Map health check endpoint
app.MapHealthChecks("/health");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
