# K8s Compliment Shop

A microservices-based e-commerce application for shopping compliments and positive affirmations. Built to strengthen Kubernetes and modern C# skills, featuring StatefulSets, gRPC, Blazor, and JSON-LD.

## Project Goals

- **Kubernetes Mastery**: Practice StatefulSets, PV/PVC, Services, Deployments, and other K8s concepts for CKA/CKAD/CKS exam prep
- **Modern C#**: Utilize .NET 8 LTS with LINQ, async/await, and current language features
- **Microservices Architecture**: Implement BFF pattern, gRPC communication, and event-driven design
- **Learning Technologies**: gRPC, Kafka, JSON-LD, Blazor, and GKE deployment

## Technology Stack

- **.NET 8 LTS** - Long-term support version
- **gRPC** - Service-to-service communication
- **Blazor** - C# frontend framework
- **MySQL 8.0** - Shared database with StatefulSet
- **Apache Kafka** - Event-driven messaging
- **JSON-LD** - Schema.org structured data for SEO
- **Kubernetes** - Container orchestration
- **Docker Desktop** - Local development environment
- **GKE** - Production deployment target

## Architecture

### Microservices

```
┌─────────────┐
│ web-service │ (Blazor UI - User shopping interface)
└──────┬──────┘
       │ HTTP
       ↓
┌─────────────┐
│ bff-service │ (Backend for Frontend - API Gateway)
└──────┬──────┘
       │ gRPC
       ├──────────────┬─────────────┐
       ↓              ↓             ↓
┌──────────────┐ ┌────────────┐ ┌──────────────┐
│product-service│ │order-service│ │ ad-service   │
└──────┬───────┘ └─────┬──────┘ └──────────────┘
       │               │
       ↓               ↓ (Kafka)
   ┌────────────────────┐
   │   MySQL (StatefulSet)│
   └────────────────────┘
```

### Services Overview

1. **product-service** ✅ - Manages compliment catalog with JSON-LD support
2. **order-service** 🔲 - Processes orders asynchronously via Kafka, fulfills with product details
3. **bff-service** 🔲 - Backend for Frontend, single entry point for web-service
4. **web-service** 🔲 - Blazor UI with Apple Liquid Glass-inspired design
5. **ad-service** 🔲 - Displays ads for personal projects/tools (no real currency)

## Repository Structure

```
k8s-compliment-shop/
├── services/
│   ├── product-service/      ✅ COMPLETED
│   │   ├── src/ProductService/
│   │   ├── tests/ProductService.Tests/
│   │   ├── scripts/          (Helper scripts)
│   │   ├── k8s/
│   │   ├── Dockerfile
│   │   └── ProductService.sln
│   ├── order-service/         🔲 TODO
│   ├── bff-service/           🔲 TODO
│   ├── web-service/           🔲 TODO
│   └── ad-service/            🔲 TODO
├── infrastructure/
│   ├── mysql/                 🔲 TODO (StatefulSet)
│   └── kafka/                 🔲 TODO (with Zookeeper)
├── docs/                      ✅ Documentation
│   ├── DOTNET_FOR_JAVA_DEVS.md
│   └── CHEATSHEET.md
└── README.md
```

## Design Principles

### Mono-Repo with Service Boundaries

Each service is **completely isolated** with its own solution file:
- ❌ No cross-service code references
- ❌ No shared common libraries (for this project)
- ✅ All communication via gRPC or Kafka
- ✅ Each service acts as if in separate GitHub repo

### Database Strategy

**Shared MySQL Database** running as a StatefulSet:
- Single source of truth for learning PV/PVC concepts
- Real-world challenge of managing state in Kubernetes
- Practice with StatefulSet ordering and stable network identities

### Communication Patterns

- **Synchronous**: gRPC for request/response (BFF → Services)
- **Asynchronous**: Kafka for events (Order creation → Fulfillment)
- **External**: HTTP/REST for web browser → BFF

## Documentation

📚 **New to .NET from Java?** Check out our comprehensive guides:

- **[.NET for Java Developers](docs/DOTNET_FOR_JAVA_DEVS.md)** - Complete comparison guide
  - Project GUIDs explained (those random alphanumeric strings!)
  - Maven → dotnet CLI mapping
  - JPA → Entity Framework Core
  - JUnit → xUnit
  - Common patterns and gotchas

- **[.NET CLI Cheat Sheet](docs/CHEATSHEET.md)** - Quick command reference
  - All dotnet commands with Maven equivalents
  - Testing, building, publishing workflows
  - Entity Framework migrations
  - Helper scripts usage

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [kubectl](https://kubernetes.io/docs/tasks/tools/)
- [JetBrains Rider](https://www.jetbrains.com/rider/) (recommended IDE)
- [grpcurl](https://github.com/fullstorydev/grpcurl) (for testing gRPC services)

### Current Status

**product-service** is complete and ready for deployment:

1. **Build the service**:
   ```bash
   cd services/product-service
   docker build -t product-service:latest .
   ```

2. **Deploy MySQL** (coming soon in `infrastructure/mysql/`)

3. **Deploy to Kubernetes**:
   ```bash
   kubectl apply -f services/product-service/k8s/
   ```

4. **Test with grpcurl**:
   ```bash
   kubectl port-forward svc/product-service 8080:8080
   grpcurl -plaintext localhost:8080 list
   ```

See [product-service/README.md](services/product-service/README.md) for detailed service documentation.

## JSON-LD Integration

Products expose Schema.org-compliant JSON-LD for SEO:
- Product catalog markup
- Enables rich search results
- Improves discoverability
- Real-world e-commerce best practice

Example:
```json
{
  "@context": "https://schema.org",
  "@type": "Product",
  "name": "Excellent Haircut Compliment",
  "category": "Appearance",
  "offers": {
    "@type": "Offer",
    "price": "0.00",
    "priceCurrency": "USD"
  }
}
```

## Compliments Catalog

All compliments are **hard-coded in C#** (no AI generation):
- Appearance: Haircut, Outfit, Smile, etc.
- Professional: Work Quality, Leadership, Creativity
- Personal: Kindness, Humor, Positivity

## Next Steps

1. ✅ ~~product-service~~ (COMPLETE)
2. 🔲 MySQL StatefulSet with PV/PVC
3. 🔲 order-service with Kafka integration
4. 🔲 bff-service as API gateway
5. 🔲 web-service with Blazor UI
6. 🔲 Deploy to GKE
7. 🔲 Add Google Analytics 4
8. 🔲 Optional: ad-service

## Learning Objectives

- **Kubernetes**: StatefulSets, PV/PVC, Services, ConfigMaps, Secrets
- **C# Modern Features**: LINQ, async/await, pattern matching, records
- **gRPC**: Protocol Buffers, service definitions, client/server implementation
- **Event-Driven**: Kafka producers/consumers, message patterns
- **Frontend**: Blazor components, routing, state management
- **Cloud Native**: Health checks, graceful shutdown, 12-factor app principles

## License

This is a learning project. Feel free to use for educational purposes.

## Author

Built by Justin Wheeler as a hands-on learning project for Kubernetes certification prep and modern C# exploration.
