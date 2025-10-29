# Web Service - Blazor Web App

A modern Blazor Web App with Apple-inspired Liquid Glass UI for the Compliment Shop.

## Features

### UI/UX
- **Liquid Glass Design** - iOS-inspired frosted glass effect with backdrop blur
- **Smooth Animations** - Fade-in, slide-up transitions for premium feel
- **Skeleton Loaders** - Shimmer effects during data loading
- **Responsive Design** - Mobile-first responsive layout

### Functionality
- **Product Listing** - Browse compliments with category filtering
- **Product Details** - Detailed product view with rich information
- **JSON-LD SEO** - Structured data injected in `<head>` for search engines
- **Google Analytics 4** - Page view and product view tracking

### Architecture
- **.NET 8 Blazor Web App** - Hybrid SSR + Interactive Server components
- **StrawberryShake** - GraphQL client for BFF communication
- **Health Checks** - Kubernetes readiness and liveness probes
- **Configuration** - Environment-based settings via appsettings.json

## Technology Stack

- **Framework**: .NET 8 Blazor Web App
- **GraphQL Client**: StrawberryShake 15.1.11
- **Rendering**: Server-side with interactive components
- **Styling**: Custom CSS with glassmorphism effects
- **Analytics**: Google Analytics 4 (gtag.js)

## Project Structure

```
src/WebService/
├── Components/
│   ├── Layout/
│   │   └── MainLayout.razor          # Main layout with header/footer
│   ├── Pages/
│   │   ├── Home.razor                # Product listing with filters
│   │   └── ProductDetail.razor       # Product detail with JSON-LD
│   ├── Shared/
│   │   ├── ProductCard.razor         # Glass card component
│   │   └── SkeletonLoader.razor      # Loading skeleton
│   └── App.razor                     # Root component
├── GraphQL/
│   ├── schema.graphql                # BFF GraphQL schema
│   ├── GetProducts.graphql           # Products list query
│   └── GetProduct.graphql            # Single product query
├── Services/
│   └── AnalyticsService.cs           # GA4 integration service
├── wwwroot/
│   ├── css/
│   │   ├── liquid-glass.css          # Glassmorphism styles
│   │   └── app.css                   # App-specific styles
│   └── js/
│       └── analytics.js              # GA4 JavaScript helper
├── Program.cs                        # Application entry point
└── appsettings.json                  # Configuration
```

## Configuration

### appsettings.json

```json
{
  "BffService": {
    "Url": "http://localhost:8082/graphql"
  },
  "GoogleAnalytics": {
    "MeasurementId": "G-XXXXXXXXXX"
  }
}
```

### Environment Variables (Kubernetes)

- `BffService__Url` - BFF GraphQL endpoint (default: http://bff-service:8082/graphql)
- `GoogleAnalytics__MeasurementId` - GA4 measurement ID

## Local Development

### Prerequisites

- .NET 8 SDK
- BFF Service running on http://localhost:8082

### Run Locally

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run --project src/WebService/WebService.csproj
```

The application will be available at `http://localhost:5000`.

### Development with Hot Reload

```bash
dotnet watch --project src/WebService/WebService.csproj
```

## Docker

### Build Image

```bash
docker build -t web-service:latest .
```

### Run Container

```bash
docker run -d \
  -p 8080:8080 \
  -e BffService__Url=http://bff-service:8082/graphql \
  -e GoogleAnalytics__MeasurementId=G-XXXXXXXXXX \
  --name web-service \
  web-service:latest
```

### Health Check

```bash
curl http://localhost:8080/health
```

## Kubernetes Deployment

### Apply Manifests

```bash
# Apply ConfigMap (update GA measurement ID first)
kubectl apply -f k8s/configmap.yaml

# Apply Service
kubectl apply -f k8s/service.yaml

# Apply Deployment
kubectl apply -f k8s/deployment.yaml
```

### Verify Deployment

```bash
# Check pod status
kubectl get pods -l app=web-service

# Check service
kubectl get svc web-service

# View logs
kubectl logs -l app=web-service --tail=100
```

### Port Forward (Local Testing)

```bash
kubectl port-forward svc/web-service 8080:8080
```

## Liquid Glass UI

### Design System

The Liquid Glass UI is inspired by Apple's iOS design language:

- **Colors**: Subtle gradients (light blue to white)
- **Glass Effect**: `backdrop-filter: blur(20px)` with semi-transparent backgrounds
- **Shadows**: Layered, soft shadows for depth
- **Borders**: 1px semi-transparent white borders
- **Animations**: 0.3s ease-in-out transitions
- **Typography**: System fonts (San Francisco-style)

### CSS Variables

```css
--glass-bg: rgba(255, 255, 255, 0.7)
--blur-amount: 20px
--border-radius-md: 16px
--shadow-md: 0 4px 16px rgba(0, 0, 0, 0.1)
```

### Components

- `.glass-panel` - Basic glass container
- `.glass-card` - Interactive glass card with hover effects
- `.skeleton` - Loading skeleton with shimmer animation
- `.btn-glass` - Glass-style button

## Google Analytics Integration

### Setup

1. Create a GA4 property at https://analytics.google.com
2. Copy your Measurement ID (format: `G-XXXXXXXXXX`)
3. Update `appsettings.json` or ConfigMap with your ID

### Tracked Events

- **Page View** - Tracked on Home page load
- **Product View** - Tracked when viewing product details

### Custom Events

Use `AnalyticsService` to track custom events:

```csharp
await Analytics.TrackEventAsync("custom_event", new {
    param1 = "value1",
    param2 = "value2"
});
```

## GraphQL Client

The app uses StrawberryShake to generate a strongly-typed GraphQL client.

### Regenerate Client

If you modify GraphQL schema or queries:

```bash
dotnet build
```

The client is automatically regenerated during build.

### Example Usage

```csharp
@inject IComplimentShopClient GraphQLClient

var result = await GraphQLClient.GetProducts.ExecuteAsync(category, page, pageSize);
if (result.Data != null)
{
    var products = result.Data.Products?.Items;
}
```

## Performance

- **Resource Limits**: 512Mi memory, 500m CPU
- **Resource Requests**: 256Mi memory, 250m CPU
- **Replicas**: 2 (for high availability)
- **Probes**: Liveness (30s interval), Readiness (10s interval)

## Troubleshooting

### Build Errors

```bash
# Clean and rebuild
dotnet clean
dotnet build
```

### GraphQL Client Issues

```bash
# Remove generated files and rebuild
rm -rf obj/ bin/
dotnet build
```

### Connection Issues

Check BFF service URL:

```bash
# In Kubernetes
kubectl logs -l app=web-service | grep "BffService"

# Check connectivity
kubectl exec -it <web-service-pod> -- curl http://bff-service:8082/graphql
```

## Next Steps

1. **Ingress Setup** - Configure Ingress to expose web-service externally
2. **SSL/TLS** - Add Let's Encrypt certificate via cert-manager
3. **CDN** - Consider CloudFlare or similar for static assets
4. **Monitoring** - Add Application Insights or Prometheus metrics
5. **Testing** - Add unit tests and integration tests

## Contributing

When making changes:

1. Update GraphQL queries in `GraphQL/*.graphql`
2. Rebuild to regenerate client
3. Update Kubernetes ConfigMap if configuration changes
4. Test locally before deploying

## License

Part of the k8s-compliment-shop project.
