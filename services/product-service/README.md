# Product Service

A gRPC-based microservice for managing the compliment product catalog. Built with .NET 8, Entity Framework Core, and MySQL.

## Features

- **gRPC API** for high-performance service-to-service communication
- **MySQL Database** integration with Entity Framework Core
- **JSON-LD Support** for Schema.org Product markup (SEO-friendly)
- **CRUD Operations** for compliment products
- **Health Checks** for Kubernetes readiness/liveness probes
- **Containerized** with multi-stage Docker builds

## Technology Stack

- .NET 8 (LTS)
- gRPC with Protocol Buffers
- Entity Framework Core 8
- Pomelo MySQL Provider
- Newtonsoft.Json (JSON-LD serialization)

## Project Structure

```
product-service/
├── src/
│   └── ProductService/
│       ├── Data/              # EF Core DbContext
│       ├── Models/            # Domain models (Product, ProductJsonLd)
│       ├── Protos/            # gRPC service definitions (.proto files)
│       ├── Repositories/      # Data access layer
│       ├── Services/          # gRPC service implementations
│       └── Program.cs         # Application entry point
├── k8s/                       # Kubernetes manifests
├── Dockerfile                 # Multi-stage container build
└── ProductService.sln         # Solution file
```

## gRPC Services

The service exposes the following gRPC methods:

- `GetProduct` - Retrieve a single product by ID
- `ListProducts` - List products with optional category filter and pagination
- `CreateProduct` - Create a new compliment product
- `UpdateProduct` - Update an existing product
- `DeleteProduct` - Remove a product from the catalog
- `GetProductJsonLd` - Get product in Schema.org JSON-LD format

## JSON-LD Integration

Each product can be converted to Schema.org JSON-LD format for SEO purposes:

```json
{
  "@context": "https://schema.org",
  "@type": "Product",
  "productID": "1",
  "name": "Excellent Haircut Compliment",
  "description": "A genuine compliment about your fantastic hairstyle",
  "category": "Appearance",
  "offers": {
    "@type": "Offer",
    "price": "0.00",
    "priceCurrency": "USD",
    "availability": "https://schema.org/InStock"
  }
}
```

## Database Schema

The service uses a single `Products` table with the following schema:

- `Id` (int, primary key, auto-increment)
- `Name` (varchar 200, required)
- `Description` (varchar 1000, required)
- `Category` (varchar 100, required, indexed)
- `CreatedAt` (datetime)
- `UpdatedAt` (datetime)

## Development

### Prerequisites

- .NET 8 SDK
- MySQL Server (or use Docker)
- Docker Desktop (for containerization)

### Local Development

1. **Update connection string** in `appsettings.Development.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Port=3306;Database=complimentshop;User=root;Password=yourpassword;"
   }
   ```

2. **Run MySQL** locally:
   ```bash
   docker run --name mysql -e MYSQL_ROOT_PASSWORD=yourpassword -p 3306:3306 -d mysql:8.0
   ```

3. **Build and run**:
   ```bash
   cd src/ProductService
   dotnet restore
   dotnet build
   dotnet run
   ```

4. **Test with grpcurl**:
   ```bash
   # List services
   grpcurl -plaintext localhost:8080 list

   # Get a product
   grpcurl -plaintext -d '{"id": 1}' localhost:8080 product.ProductService/GetProduct

   # List all products
   grpcurl -plaintext -d '{}' localhost:8080 product.ProductService/ListProducts

   # Get JSON-LD format
   grpcurl -plaintext -d '{"id": 1}' localhost:8080 product.ProductService/GetProductJsonLd
   ```

## Docker

### Build Image

```bash
docker build -t product-service:latest .
```

### Run Container

```bash
docker run -p 8080:8080 -p 8081:8081 \
  -e ConnectionStrings__DefaultConnection="Server=host.docker.internal;Port=3306;Database=complimentshop;User=root;Password=yourpassword;" \
  product-service:latest
```

## Kubernetes Deployment

### Prerequisites

- Kubernetes cluster (Docker Desktop, Minikube, or GKE)
- MySQL StatefulSet deployed (see `infrastructure/mysql/`)
- kubectl configured

### Deploy

```bash
# Create secret
kubectl apply -f k8s/secret.yaml

# Deploy service
kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/service.yaml

# Verify deployment
kubectl get pods -l app=product-service
kubectl get svc product-service
kubectl logs -l app=product-service
```

### Health Checks

The service exposes a health check endpoint at `/health` (HTTP on port 8080) that validates:
- Application is running
- Database connection is healthy

## Seed Data

The service automatically seeds 6 compliment products on first run:

1. Excellent Haircut Compliment (Appearance)
2. Professional Outfit Compliment (Appearance)
3. Brilliant Work Compliment (Professional)
4. Creative Thinking Compliment (Professional)
5. Kind Heart Compliment (Personal)
6. Great Sense of Humor Compliment (Personal)

## Environment Variables

- `ASPNETCORE_ENVIRONMENT` - Environment (Development/Production)
- `ASPNETCORE_URLS` - HTTP endpoint binding
- `ConnectionStrings__DefaultConnection` - MySQL connection string

## Next Steps

This service will be consumed by:
- **bff-service** - Backend for Frontend (gRPC client)
- **order-service** - Order fulfillment (gRPC client)
- **web-service** - Blazor UI (via BFF)
