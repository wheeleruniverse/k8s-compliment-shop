using WebService.Components;
using WebService.GraphQL;
using WebService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire ServiceDefaults (health checks, telemetry, service discovery)
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register analytics service
builder.Services.AddScoped<AnalyticsService>();

// Configure GraphQL client for BFF service
// Use Aspire service discovery (dev) or environment variable (K8s) or fallback
var bffServiceUrl = builder.Configuration.GetValue<string>("services:bff-service:https:0")
    ?? builder.Configuration.GetValue<string>("services:bff-service:http:0")
    ?? builder.Configuration["BffService:Url"]
    ?? "http://localhost:8082";

// Ensure URL ends with /graphql
var graphqlUrl = bffServiceUrl.EndsWith("/graphql") ? bffServiceUrl : $"{bffServiceUrl}/graphql";

builder.Services
    .AddComplimentShopClient()
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri(graphqlUrl);
    });

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

// Map Aspire health endpoints
app.MapDefaultEndpoints();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
