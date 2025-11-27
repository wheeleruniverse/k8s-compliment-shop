# K8s Compliment Shop

A microservices-based e-commerce application for shopping compliments and positive affirmations. Built to strengthen Kubernetes and modern C# skills, featuring StatefulSets, gRPC, Blazor, and automated CI/CD.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Kubernetes](https://img.shields.io/badge/Kubernetes-GKE-326CE5)](https://cloud.google.com/kubernetes-engine)

---

## 📋 Table of Contents

- [Overview](#overview)
- [Technology Stack](#technology-stack)
- [Architecture](#architecture)
- [Quick Start](#quick-start)
  - [Prerequisites](#prerequisites)
  - [Automated Deployment (Recommended)](#automated-deployment-recommended)
  - [Local Testing](#local-testing)
- [CI/CD Pipeline](#cicd-pipeline)
- [GitHub Secrets Setup](#github-secrets-setup)
- [Kubernetes Deployment](#kubernetes-deployment)
  - [Helm + Kustomize + ArgoCD](#helm--kustomize--argocd)
  - [Manifest Vendoring](#manifest-vendoring)
- [Project Structure](#project-structure)
- [Service Details](#service-details)
- [Database & StatefulSets](#database--statefulsets)
- [Documentation](#documentation)
- [Learning Objectives](#learning-objectives)
- [Contributing](#contributing)

---

## Overview

### Project Goals

- **Kubernetes Mastery**: Practice StatefulSets, PV/PVC, Services, Deployments for CKA/CKAD/CKS exam prep
- **Modern C#**: Utilize .NET 8 LTS with LINQ, async/await, and current language features
- **Microservices Architecture**: Implement BFF pattern, gRPC communication, and event-driven design
- **GitOps & CI/CD**: Automated builds, immutable image tags, and GitOps with ArgoCD
- **Production Practices**: Health checks, graceful shutdown, observability, security best practices

### Current Status

✅ **Production-Ready CI/CD Pipeline**
- Automated builds on push to main
- Immutable Docker image tags (commit SHA)
- Automatic Helm chart updates
- Manifest vendoring for PR visibility
- Deployment to GKE with secrets management

---

## Technology Stack

| Category | Technology |
|----------|-----------|
| **Runtime** | .NET 8 LTS |
| **Communication** | gRPC (service-to-service), GraphQL (BFF), HTTP/REST (external) |
| **Frontend** | Blazor WebAssembly |
| **Database** | MySQL 8.0 (StatefulSet with PV/PVC) |
| **Messaging** | Apache Kafka (event-driven) |
| **Container Registry** | Google Artifact Registry |
| **Orchestration** | Kubernetes (GKE) |
| **Package Management** | Helm 3 |
| **Configuration** | Kustomize |
| **GitOps** | ArgoCD |
| **CI/CD** | GitHub Actions |
| **SEO** | JSON-LD (Schema.org) |

---

## Architecture

### Microservices Overview

```
┌─────────────┐
│ web-service │ (Blazor UI - User shopping interface)
└──────┬──────┘
       │ HTTP
       ↓
┌─────────────┐
│ bff-service │ (Backend for Frontend - GraphQL API Gateway)
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
   │   MySQL StatefulSet │
   └────────────────────┘
```

### Services

1. **web-service** - Blazor WebAssembly UI with Apple Liquid Glass-inspired design
2. **bff-service** - GraphQL Backend-for-Frontend, single entry point for frontend
3. **product-service** - gRPC API managing compliment catalog with JSON-LD support
4. **order-service** - Processes orders asynchronously via Kafka
5. **ad-service** - Displays ads for personal projects (no real currency)

### Communication Patterns

- **Synchronous**: gRPC for request/response (BFF → Services)
- **Asynchronous**: Kafka for events (Order creation → Fulfillment)
- **External**: HTTP/REST for web browser → BFF
- **Frontend**: GraphQL for flexible data fetching

---

## Quick Start

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [kubectl](https://kubernetes.io/docs/tasks/tools/)
- [Helm](https://helm.sh/docs/intro/install/)
- [gcloud CLI](https://cloud.google.com/sdk/docs/install) (for GKE)
- [grpcurl](https://github.com/fullstorydev/grpcurl) (for testing gRPC)

### Automated Deployment (Recommended)

**The fastest way to deploy is using our automated GitHub Actions workflow!**

The CI/CD pipeline automatically:
1. ✅ Builds Docker images when code changes
2. ✅ Tags with commit SHA (immutable, traceable)
3. ✅ Pushes to Google Artifact Registry
4. ✅ Updates Helm values automatically
5. ✅ Vendors manifests for PR review
6. ✅ Deploys to your GKE cluster

**Setup:**

1. **Configure GitHub Secrets** (one-time setup):
   ```bash
   # Go to: Settings → Secrets and variables → Actions

   # Add secrets:
   - GCP_SERVICE_ACCOUNT_KEY  # Your GCP service account JSON key
   - MYSQL_ROOT_PASSWORD      # Secure MySQL password

   # Add variables:
   - GCP_PROJECT_ID           # Your GCP project ID
   - GCP_REGION               # us-central1
   - GCP_ARTIFACT_REGISTRY_REPO  # k8s-compliment-shop
   ```

2. **Configure your GKE cluster** in `.github/workflows/build-and-deploy.yaml`:
   ```yaml
   # Line ~234 - Update with your cluster name
   gcloud container clusters get-credentials YOUR_CLUSTER_NAME \
     --region ${{ vars.GCP_REGION }} \
     --project ${{ vars.GCP_PROJECT_ID }}

   # Line ~246 - Uncomment the helm upgrade command
   ```

3. **Push code to trigger deployment**:
   ```bash
   git push origin main
   # Watch the magic happen in GitHub Actions!
   ```

**Image Tagging Strategy:**

Every commit gets a unique, immutable tag:
```
Commit SHA: abc123f4567890...
↓
Image: us-central1-docker.pkg.dev/PROJECT/REPO/web-service:abc123f
                                                              ↑
                                                        Immutable & Traceable
```

**Benefits:**
- Know exactly which code is running
- Rollback to any previous commit
- Full audit trail
- No accidental overwrites (immutable)

### Local Testing

Test services locally with Docker before deploying to Kubernetes:

```bash
# 1. Start MySQL
docker run -d \
  --name mysql \
  -e MYSQL_ROOT_PASSWORD=yourpassword \
  -e MYSQL_DATABASE=complimentshop \
  -p 3306:3306 \
  mysql:8.0

# 2. Build service
cd services/product-service
docker build -t product-service:latest .

# 3. Run service
docker run -d \
  --name product-service \
  --link mysql:mysql \
  -p 8080:8080 \
  -p 8081:8081 \
  -e ConnectionStrings__DefaultConnection="Server=mysql;Port=3306;Database=complimentshop;User=root;Password=yourpassword;" \
  product-service:latest

# 4. Test health
curl http://localhost:8080/health

# 5. Test gRPC
grpcurl -plaintext localhost:8080 list
grpcurl -plaintext -d '{"id": 1}' localhost:8080 product.ProductService/GetProduct

# 6. Cleanup
docker stop product-service mysql
docker rm product-service mysql
```

**Testing Checklist:**
- [ ] Health endpoint returns "Healthy"
- [ ] gRPC reflection working
- [ ] GetProduct returns seeded data
- [ ] ListProducts returns all products
- [ ] CreateProduct adds new product
- [ ] Data persists in MySQL

---

## CI/CD Pipeline

### Automated Build & Deploy Workflow

**File:** `.github/workflows/build-and-deploy.yaml`

**Triggers:**
- Push to `main` branch (when services or Helm charts change)
- Manual via workflow dispatch

**What it does:**

```
1. Detect Changes → Which services changed?
   ↓
2. Build & Push → Docker images to Artifact Registry
   ↓
3. Tag Images → Commit SHA (abc123f) + latest
   ↓
4. Update Helm → Update values.yaml with new tags
   ↓
5. Vendor Manifests → Re-render to k8s/rendered/
   ↓
6. Commit Back → Auto-commit updated manifests
   ↓
7. Deploy → Helm upgrade to GKE cluster
   ↓
8. Summary → Show what was deployed
```

**Manual Triggers:**

```bash
# Deploy all services
gh workflow run build-and-deploy.yaml

# Deploy specific service
gh workflow run build-and-deploy.yaml -f service=web-service

# Rollback to previous commit
./scripts/update-image-tags.sh all <old-sha>
./scripts/vendor-manifests.sh
git add k8s/ && git commit -m "Rollback to <old-sha>" && git push
```

**Scripts:**

- `scripts/vendor-manifests.sh` - Renders Helm + Kustomize manifests
- `scripts/update-image-tags.sh` - Updates image tags in values.yaml

---

## GitHub Secrets Setup

### Required Secrets

| Name | Type | Purpose |
|------|------|---------|
| `GCP_SERVICE_ACCOUNT_KEY` | Secret | Authenticate to GCP Artifact Registry |
| `MYSQL_ROOT_PASSWORD` | Secret | MySQL root password for production |
| `GITHUB_TOKEN` | Auto-provided | GitHub Actions automation |

### Required Variables

| Name | Value | Purpose |
|------|-------|---------|
| `GCP_PROJECT_ID` | Your project ID | Reference GCP project |
| `GCP_REGION` | us-central1 | Artifact Registry region |
| `GCP_ARTIFACT_REGISTRY_REPO` | k8s-compliment-shop | Registry repo name |

### Setup Instructions

**1. Create GCP Service Account:**

```bash
# Create service account
gcloud iam service-accounts create github-actions \
  --project=YOUR_PROJECT_ID

# Grant Artifact Registry Writer role
gcloud projects add-iam-policy-binding YOUR_PROJECT_ID \
  --member="serviceAccount:github-actions@YOUR_PROJECT_ID.iam.gserviceaccount.com" \
  --role="roles/artifactregistry.writer"

# Create key
gcloud iam service-accounts keys create key.json \
  --iam-account=github-actions@YOUR_PROJECT_ID.iam.gserviceaccount.com

# Copy contents to GitHub secret, then delete local key
cat key.json
rm key.json
```

**2. Add Secrets to GitHub:**

```
Settings → Secrets and variables → Actions → New repository secret
```

**3. Generate MySQL Password:**

```bash
openssl rand -base64 32
```

### Security Best Practices

- ✅ Secrets configured in GitHub (never in code)
- ✅ `values.prod.yaml` in `.gitignore` (local prod values)
- ✅ Placeholder values in vendored manifests (`PLACEHOLDER_REPLACE_AT_DEPLOY_TIME`)
- ✅ Rotate secrets every 90 days
- ✅ Use least-privilege service accounts

---

## Kubernetes Deployment

### Helm + Kustomize + ArgoCD

**Architecture:**

```
Git Push → GitHub Actions → Vendor Manifests → ArgoCD → Kubernetes
```

**1. Helm (Templating)**
- Templates Kubernetes manifests
- Manages versions and configuration
- Source: `k8s/helm/`

**2. Kustomize (Patching)**
- Adds common labels/annotations
- Applies environment-specific patches
- Source: `k8s/helm/kustomize/`

**3. ArgoCD (GitOps)**
- Watches Git repository
- Automatically syncs to cluster
- Self-heals on drift
- Config: `k8s/argocd/`

### Manifest Vendoring

**Why vendor manifests?**
- ✅ Full visibility in PRs of what will be deployed
- ✅ Audit trail of infrastructure changes
- ✅ Review exact manifests before merge
- ✅ Safe for public repos (uses placeholders)

**How it works:**

```bash
# Automatic (via GitHub Actions)
git push → Workflow renders → Commits to k8s/rendered/

# Manual
./scripts/vendor-manifests.sh
git diff k8s/rendered/  # Review changes
git add k8s/rendered/
git commit -m "Update manifests"
```

**Files:**
- `k8s/helm/values.yaml` - Base values (placeholder password)
- `k8s/helm/values.public.yaml` - Public-safe values for vendoring
- `k8s/rendered/` - Vendored manifests (auto-generated)

**Security:**
- Vendored manifests use `PLACEHOLDER_REPLACE_AT_DEPLOY_TIME`
- Real secrets provided at deployment via `--set` or private values file
- `values.prod.yaml` in `.gitignore`

### Deployment Options

**Option 1: Automated (GitHub Actions)**
```bash
git push origin main  # Deploys automatically
```

**Option 2: Manual with Helm**
```bash
helm upgrade --install k8s-compliment-shop k8s/helm \
  --set mysql.auth.rootPassword="$MYSQL_ROOT_PASSWORD" \
  --namespace default
```

**Option 3: ArgoCD (GitOps)**
```bash
kubectl apply -f k8s/argocd/application.yaml
# ArgoCD syncs automatically
```

---

## Project Structure

```
k8s-compliment-shop/
├── .github/workflows/
│   ├── build-and-deploy.yaml      # Main CI/CD workflow
│   ├── vendor-manifests.yaml      # Manifest vendoring
│   └── README.md                  # Workflow documentation
├── services/
│   ├── web-service/               # Blazor frontend
│   │   ├── src/WebService/
│   │   ├── tests/
│   │   ├── Dockerfile
│   │   └── README.md
│   ├── bff-service/               # GraphQL BFF
│   │   └── [same structure]
│   ├── product-service/           # gRPC product API
│   │   └── [same structure]
│   ├── order-service/             # Kafka order processor
│   └── ad-service/                # Ad display service
├── k8s/
│   ├── helm/                      # Helm chart (source of truth)
│   │   ├── Chart.yaml
│   │   ├── values.yaml           # Base values
│   │   ├── values.public.yaml    # Public-safe values
│   │   ├── templates/            # K8s templates
│   │   └── kustomize/            # Kustomize patches
│   ├── argocd/                   # ArgoCD config
│   └── rendered/                 # Vendored manifests (auto-generated)
├── scripts/
│   ├── vendor-manifests.sh       # Vendor Helm + Kustomize
│   └── update-image-tags.sh      # Update image tags
├── docs/
│   ├── DOTNET_FOR_JAVA_DEVS.md   # Java → .NET guide
│   └── CHEATSHEET.md             # .NET CLI cheat sheet
└── README.md                      # This file
```

---

## Service Details

### product-service ✅

**Status:** Complete and production-ready

**Technology:**
- gRPC API
- Entity Framework Core
- MySQL database
- JSON-LD (Schema.org) support

**Endpoints:**
- `GetProduct` - Get product by ID
- `ListProducts` - List all products with filtering
- `CreateProduct` - Create new product
- `UpdateProduct` - Update existing product
- `DeleteProduct` - Delete product
- `GetProductJsonLd` - Get JSON-LD format

**Compliments Catalog:**
- **Appearance**: Haircut, Outfit, Smile, Eyes
- **Professional**: Work Quality, Leadership
- **Personal**: Kindness, Humor

**Testing:**
```bash
grpcurl -plaintext -d '{"id": 1}' localhost:8080 product.ProductService/GetProduct
```

### bff-service 🔲

GraphQL Backend-for-Frontend serving as API gateway.

### web-service 🔲

Blazor WebAssembly UI with Apple Liquid Glass-inspired design.

### order-service 🔲

Processes orders asynchronously via Kafka.

### ad-service 🔲

Displays ads for personal projects (no real currency).

---

## Database & StatefulSets

### MySQL StatefulSet

**Configuration:**
- Version: MySQL 8.0
- Storage: Persistent Volume Claims (PVC)
- Size: 10Gi (configurable in `values.yaml`)
- Storage Class: `standard-rwo` (GKE)

**Features:**
- Stable network identity
- Persistent storage
- Ordered deployment
- Graceful scaling

**Connection:**
```
Host: mysql-service
Port: 3306
Database: complimentshop
User: root
Password: <from secret>
```

**Schema:**
```sql
Products (
  Id INT PRIMARY KEY,
  Name VARCHAR(255),
  Description TEXT,
  Category VARCHAR(50),
  Price DECIMAL(10,2),
  Currency VARCHAR(3),
  IsAvailable BOOL,
  CreatedAt DATETIME,
  UpdatedAt DATETIME
)
```

---

## Documentation

### For Developers

- **[.NET for Java Developers](docs/DOTNET_FOR_JAVA_DEVS.md)** - Complete .NET guide for Java developers
  - Project GUIDs explained
  - Maven → dotnet CLI mapping
  - JPA → Entity Framework Core
  - JUnit → xUnit
  - Common patterns and gotchas

- **[.NET CLI Cheat Sheet](docs/CHEATSHEET.md)** - Quick command reference
  - All dotnet commands
  - Testing, building, publishing
  - Entity Framework migrations
  - Helper scripts

### For Operations

- **[Workflow Documentation](.github/workflows/README.md)** - Complete CI/CD guide
  - Workflow details
  - Manual triggers
  - Troubleshooting
  - Rollback procedures

- **[Kubernetes Guide](k8s/README.md)** - K8s deployment details
  - Helm + Kustomize + ArgoCD architecture
  - Deployment options
  - Configuration management

### Quick References

```bash
# Build service
cd services/product-service
dotnet build

# Run tests
dotnet test

# Run locally
dotnet run --project src/ProductService

# Docker build
docker build -t product-service:latest .

# Update image tags
./scripts/update-image-tags.sh web-service abc123f

# Vendor manifests
./scripts/vendor-manifests.sh

# Deploy with Helm
helm upgrade --install k8s-compliment-shop k8s/helm \
  --set mysql.auth.rootPassword="$MYSQL_ROOT_PASSWORD"

# Trigger workflow
gh workflow run build-and-deploy.yaml
```

---

## Learning Objectives

### Kubernetes

- ✅ StatefulSets with persistent storage
- ✅ Services (ClusterIP, LoadBalancer, Headless)
- ✅ Deployments with health checks
- ✅ ConfigMaps and Secrets management
- ✅ Helm package management
- ✅ Kustomize for configuration
- ✅ GitOps with ArgoCD
- 🔲 Network Policies
- 🔲 RBAC
- 🔲 Horizontal Pod Autoscaling

### C# & .NET

- ✅ .NET 8 LTS features
- ✅ gRPC client/server
- ✅ Entity Framework Core
- ✅ Async/await patterns
- ✅ LINQ queries
- ✅ xUnit testing
- 🔲 GraphQL with Hot Chocolate
- 🔲 Blazor WebAssembly
- 🔲 Kafka producers/consumers

### DevOps

- ✅ GitHub Actions workflows
- ✅ Docker multi-stage builds
- ✅ Image tagging strategies
- ✅ Secrets management
- ✅ Manifest vendoring
- ✅ GitOps principles
- 🔲 Prometheus monitoring
- 🔲 Grafana dashboards
- 🔲 Distributed tracing

---

## Contributing

This is primarily a learning project, but contributions are welcome!

### Development Workflow

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Make your changes
4. Run tests: `dotnet test`
5. Build Docker images: `docker build -t service-name .`
6. Test locally
7. Vendor manifests: `./scripts/vendor-manifests.sh`
8. Commit changes: `git commit -m 'Add amazing feature'`
9. Push to branch: `git push origin feature/amazing-feature`
10. Open a Pull Request

### Design Principles

- ✅ Each service is completely isolated (no shared libraries)
- ✅ Communication only via gRPC or Kafka
- ✅ Services act as if in separate repositories
- ✅ Immutable infrastructure (Docker images tagged by commit SHA)
- ✅ GitOps for deployment
- ✅ Security by default

---

## License

This is a learning project. Feel free to use for educational purposes.

## Author

Built by **Justin Wheeler** as a hands-on learning project for:
- Kubernetes certification prep (CKA/CKAD/CKS)
- Modern C# exploration
- Microservices architecture
- GitOps and CI/CD best practices

---

## 🚀 Ready to Deploy?

1. ✅ Configure GitHub secrets (see [GitHub Secrets Setup](#github-secrets-setup))
2. ✅ Update GKE cluster name in workflow
3. ✅ Push code to main branch
4. ✅ Watch GitHub Actions deploy automatically!

**Questions?** Check the [workflow docs](.github/workflows/README.md) or [Kubernetes guide](k8s/README.md).

Happy deploying! 🎉
