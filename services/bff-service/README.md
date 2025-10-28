# BFF Service (Backend for Frontend)

GraphQL API Gateway for the K8s Compliment Shop. Provides a GraphQL interface to microservices for web clients.

## Technology Stack

- **.NET 8** - LTS framework
- **ASP.NET Core** - Web framework
- **HotChocolate 13** - GraphQL server
- **gRPC Client** - Service-to-service communication
- **xUnit + Moq** - Testing
- **Docker** - Containerization

## Architecture

The BFF service follows the Backend for Frontend pattern:

```
Web Client → BFF (GraphQL) → Product Service (gRPC)
```

### Why BFF?

- **Tailored for Frontend**: GraphQL lets clients request exactly what they need
- **Protocol Translation**: Converts gRPC to GraphQL/HTTP
- **Aggregation**: Can combine data from multiple microservices (future)
- **CORS Handling**: Manages cross-origin requests for web clients
- **Rate Limiting**: (Future) Protects backend services

## GraphQL Schema

### Queries

```graphql
type Query {
  # Get single product
  product(id: Int!): Product

  # List products with optional filtering & pagination
  products(
    category: String
    page: Int = 1
    pageSize: Int = 20
  ): ProductConnection!
}

type Product {
  id: Int!
  name: String!
  description: String!
  category: String!
  createdAt: String!
  updatedAt: String!
  jsonLd: String!  # Optional - only fetched if requested
}

type ProductConnection {
  items: [Product!]!
  totalCount: Int!
  page: Int!
  pageSize: Int!
}
```

### Example Queries

**Get a single product:**
```graphql
query {
  product(id: 1) {
    id
    name
    description
    category
  }
}
```

**Get product with SEO data:**
```graphql
query {
  product(id: 1) {
    name
    jsonLd  # Only fetched when requested
  }
}
```

**List products with filtering:**
```graphql
query {
  products(category: "Appearance", page: 1, pageSize: 10) {
    totalCount
    page
    pageSize
    items {
      id
      name
      category
    }
  }
}
```

## Project Structure

```
bff-service/
├── src/BffService/
│   ├── GraphQL/
│   │   ├── Queries/
│   │   │   └── ProductQueries.cs       # GraphQL resolvers
│   │   └── Types/
│   │       ├── Product.cs              # Product type
│   │       └── ProductConnection.cs    # Pagination type
│   ├── Services/
│   │   └── ProductServiceClient.cs     # gRPC client wrapper
│   ├── Protos/
│   │   └── product.proto               # gRPC definitions
│   ├── Program.cs                      # App entry point
│   ├── appsettings.json                # Configuration
│   └── BffService.csproj
├── tests/BffService.Tests/
│   └── Unit/
│       ├── Services/ProductServiceClientTests.cs
│       └── GraphQL/ProductQueriesTests.cs
├── scripts/
│   ├── build.sh                        # Build script
│   ├── test.sh                         # Test script
│   ├── docker-build.sh                 # Docker build
│   └── run-local.sh                    # Run locally
├── Dockerfile
└── README.md
```

## Quick Start

### Prerequisites

- .NET 8 SDK (for local development)
- Docker (for containerized deployment)
- **product-service running** (required dependency)

### Local Development

**1. Start product-service first:**
```bash
cd ../product-service
./scripts/run-local.sh
```

**2. Run BFF service:**
```bash
./scripts/run-local.sh
```

**3. Open GraphQL Playground:**
```
http://localhost:8082/graphql
```

Try a query:
```graphql
query {
  products {
    totalCount
    items {
      id
      name
      category
    }
  }
}
```

### Testing

**Run all tests:**
```bash
./scripts/test.sh
```

**Watch mode (auto-rerun on changes):**
```bash
./scripts/test.sh --watch
```

**With coverage:**
```bash
./scripts/test.sh --coverage
```

### Docker

**Build image:**
```bash
./scripts/docker-build.sh
```

**Build with tests:**
```bash
./scripts/docker-build.sh --with-tests
```

**Run container:**
```bash
docker run -p 8082:8082 \
  -e ProductService__Url=http://host.docker.internal:8081 \
  bff-service:latest
```

## Configuration

### Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Environment (Development/Production) |
| `ProductService__Url` | `http://product-service:8081` | Product service gRPC endpoint |

### appsettings.json

```json
{
  "ProductService": {
    "Url": "http://product-service:8081"
  }
}
```

## Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/graphql` | POST | GraphQL endpoint |
| `/graphql` | GET | GraphQL Playground (dev only) |
| `/health` | GET | Health check |

## Health Checks

```bash
curl http://localhost:8082/health
```

Returns `Healthy` if the service is running.

## Development Workflow

```bash
# 1. Make changes to code
vim src/BffService/GraphQL/Queries/ProductQueries.cs

# 2. Run tests
./scripts/test.sh

# 3. Build
./scripts/build.sh

# 4. Run locally
./scripts/run-local.sh

# 5. Test in GraphQL Playground
open http://localhost:8080/graphql
```

## Testing GraphQL Queries

### Using GraphQL Playground (Browser)

1. Navigate to `http://localhost:8082/graphql`
2. Use the built-in editor with autocomplete
3. View schema documentation in the right panel

### Using cURL

```bash
# Get product
curl -X POST http://localhost:8082/graphql \
  -H "Content-Type: application/json" \
  -d '{"query":"{ product(id: 1) { id name description } }"}'

# List products
curl -X POST http://localhost:8082/graphql \
  -H "Content-Type: application/json" \
  -d '{"query":"{ products { totalCount items { id name } } }"}'
```

## Common Tasks

### Add a New Query

1. Add resolver method to `ProductQueries.cs`
2. Add tests to `ProductQueriesTests.cs`
3. Run tests: `./scripts/test.sh`
4. Test in GraphQL Playground

### Update Proto File

If product-service updates `product.proto`:

```bash
# Copy updated proto
cp ../product-service/src/ProductService/Protos/product.proto src/BffService/Protos/

# Rebuild
./scripts/build.sh
```

## Troubleshooting

### "Connection refused" to product-service

**Problem**: BFF can't connect to product-service

**Solution**:
```bash
# Check product-service is running
curl http://localhost:8081/health

# If not, start it
cd ../product-service && ./scripts/run-local.sh
```

### GraphQL Playground not showing

**Problem**: `/graphql` returns 404 in production

**Solution**: GraphQL Playground is development-only. Use POST requests or set:
```bash
export ASPNETCORE_ENVIRONMENT=Development
```

### gRPC deadline exceeded

**Problem**: Timeouts calling product-service

**Solution**: Check product-service logs, increase retry attempts in `Program.cs`

## Next Steps

- [ ] Add mutations for Create/Update/Delete (if needed)
- [ ] Add DataLoader for batch loading (performance)
- [ ] Add Redis caching
- [ ] Add authentication/authorization
- [ ] Add rate limiting
- [ ] Add circuit breaker pattern
- [ ] Add distributed tracing (OpenTelemetry)

## Related Services

- **product-service**: Manages compliment product data (gRPC)
- **web-service**: (Future) Blazor web UI consuming this GraphQL API

## Resources

- [HotChocolate Documentation](https://chillicream.com/docs/hotchocolate/v13)
- [GraphQL Specification](https://spec.graphql.org/)
- [gRPC .NET](https://docs.microsoft.com/en-us/aspnet/core/grpc/)
