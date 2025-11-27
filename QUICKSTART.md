# Quick Start Guide

Get k8s-compliment-shop running in minutes with automated CI/CD!

## Step 1: Configure GitHub Secrets ✅

You've already done this! Your secrets are configured:
- ✅ `GCP_SERVICE_ACCOUNT_KEY`
- ✅ `MYSQL_ROOT_PASSWORD`
- ✅ `GCP_PROJECT_ID`, `GCP_REGION`, `GCP_ARTIFACT_REGISTRY_REPO` variables

## Step 2: Configure Your GKE Cluster

Edit `.github/workflows/build-and-deploy.yaml`:

```yaml
# Line ~234 - Update with your cluster name
- name: Get GKE credentials
  run: |
    gcloud container clusters get-credentials YOUR_CLUSTER_NAME \  # ← Change this
      --region ${{ vars.GCP_REGION }} \
      --project ${{ vars.GCP_PROJECT_ID }}

# Line ~246 - Uncomment the helm upgrade command
- name: Deploy with Helm
  run: |
    helm upgrade --install k8s-compliment-shop k8s/helm \  # ← Uncomment these lines
      --set mysql.auth.rootPassword="${{ secrets.MYSQL_ROOT_PASSWORD }}" \
      --namespace default \
      --create-namespace \
      --wait \
      --timeout 10m
```

## Step 3: Push Code to Main

```bash
# Commit your cluster configuration
git add .github/workflows/build-and-deploy.yaml
git commit -m "Configure GKE cluster for deployment"
git push origin main
```

## Step 4: Watch the Magic Happen! ✨

1. Go to GitHub → Actions tab
2. Watch the workflow build, push, and deploy
3. See deployment summary with image tags

## Step 5: Access Your Application

```bash
# Get the LoadBalancer IP
kubectl get service web-service -n default

# Open in browser
export WEB_IP=$(kubectl get service web-service -n default -o jsonpath='{.status.loadBalancer.ingress[0].ip}')
echo "Open: http://$WEB_IP:8080"
```

---

## What Happens Automatically

### When you push code to `main`:

1. **Detects changes** - Only builds services that changed
2. **Builds Docker images** - Multi-stage builds with tests
3. **Tags with commit SHA** - Example: `abc123f` and `latest`
4. **Pushes to Artifact Registry** - Immutable tags
5. **Updates Helm values** - Automatically updates `k8s/helm/values.yaml`
6. **Vendors manifests** - Re-renders to `k8s/rendered/`
7. **Commits changes** - Bot commits updated manifests
8. **Deploys to cluster** - Uses Helm with your MySQL password

### Result:
- ✅ Code deployed within minutes
- ✅ Commit SHA traceable to running images
- ✅ Manifests always in sync
- ✅ Full audit trail in Git

---

## Manual Operations

### Manually trigger deployment:

```bash
# Deploy all services
gh workflow run build-and-deploy.yaml

# Deploy specific service
gh workflow run build-and-deploy.yaml -f service=web-service
```

### Update image tags locally:

```bash
# Update a specific service
./scripts/update-image-tags.sh web-service abc123f

# Update all services
./scripts/update-image-tags.sh all abc123f

# Vendor the changes
./scripts/vendor-manifests.sh

# Commit
git add k8s/
git commit -m "Update image tags to abc123f"
git push
```

### Rollback to previous version:

```bash
# Find previous commit
git log --oneline | head -5

# Update to old commit SHA
./scripts/update-image-tags.sh all <old-sha>
./scripts/vendor-manifests.sh

# Deploy
git add k8s/
git commit -m "Rollback to <old-sha>"
git push  # Workflow auto-deploys
```

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                      Developer Workflow                      │
└─────────────────────────────────────────────────────────────┘
                              │
                              │ git push
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     GitHub Actions                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │ Build Images │→ │ Push to GAR  │→ │ Update Helm  │     │
│  └──────────────┘  └──────────────┘  └──────────────┘     │
│                                              │               │
│  ┌──────────────┐  ┌──────────────┐         │               │
│  │ Vendor K8s   │→ │ Commit Back  │←────────┘               │
│  └──────────────┘  └──────────────┘                         │
│                           │                                  │
└───────────────────────────┼──────────────────────────────────┘
                            │
                            │ helm upgrade
                            ▼
┌─────────────────────────────────────────────────────────────┐
│               Google Kubernetes Engine (GKE)                │
│                                                              │
│  ┌────────────┐  ┌────────────┐  ┌────────────┐           │
│  │    Web     │  │    BFF     │  │  Product   │           │
│  │  Service   │→ │  Service   │→ │  Service   │           │
│  │ (Frontend) │  │ (GraphQL)  │  │   (gRPC)   │           │
│  └────────────┘  └────────────┘  └────────────┘           │
│                                          │                   │
│                                          ▼                   │
│                                   ┌────────────┐            │
│                                   │   MySQL    │            │
│                                   │ StatefulSet│            │
│                                   └────────────┘            │
└─────────────────────────────────────────────────────────────┘
```

---

## Image Tagging Strategy

Every commit gets a unique tag based on the commit SHA:

```
Commit: abc123f4567890abcdef1234567890abcdef1234
↓
Short SHA: abc123f
↓
Image Tags:
- us-central1-docker.pkg.dev/.../web-service:abc123f  ← Immutable
- us-central1-docker.pkg.dev/.../web-service:latest   ← Latest
```

**Benefits:**
- Know exactly which code is running
- Rollback to any previous commit
- Immutable - Can't be overwritten
- Full audit trail

---

## Project Structure

```
k8s-compliment-shop/
├── .github/workflows/
│   ├── build-and-deploy.yaml      # Main CI/CD workflow
│   ├── vendor-manifests.yaml      # Manifest vendoring
│   └── README.md                  # Workflow docs
├── services/
│   ├── web-service/               # Frontend (Blazor)
│   ├── bff-service/               # Backend-for-Frontend (GraphQL)
│   └── product-service/           # Product API (gRPC)
├── k8s/
│   ├── helm/                      # Helm charts (source of truth)
│   │   ├── values.yaml           # Base values
│   │   ├── values.public.yaml    # Public-safe values (placeholders)
│   │   └── templates/            # K8s templates
│   └── rendered/                  # Vendored manifests (auto-generated)
├── scripts/
│   ├── vendor-manifests.sh       # Vendor Helm+Kustomize
│   └── update-image-tags.sh      # Update image tags
├── DEPLOYMENT.md                  # Deployment guide
├── GITHUB_SECRETS.md              # Secrets setup
├── VENDORING.md                   # Vendoring guide
└── QUICKSTART.md                  # This file!
```

---

## Troubleshooting

### Workflow fails

```bash
# Check workflow logs
gh run list --workflow=build-and-deploy.yaml
gh run view <run-id>

# View in browser
# GitHub → Actions → Click on failed run
```

### Can't access application

```bash
# Check pods
kubectl get pods -n default

# Check logs
kubectl logs -n default -l app=web-service --tail=50

# Check service
kubectl get service web-service -n default

# Describe for events
kubectl describe service web-service -n default
```

### Images not pulling

```bash
# Check image pull secrets
kubectl get pods -n default -o jsonpath='{.items[0].spec.imagePullSecrets}'

# Create if missing
kubectl create secret docker-registry gcr-json-key \
  --docker-server=us-central1-docker.pkg.dev \
  --docker-username=_json_key \
  --docker-password="$(cat key.json)" \
  --namespace=default

# Patch service account
kubectl patch serviceaccount default -n default \
  -p '{"imagePullSecrets": [{"name": "gcr-json-key"}]}'
```

---

## Next Steps

1. ✅ Push code to trigger first deployment
2. 📊 Set up monitoring (Prometheus + Grafana)
3. 🔔 Configure alerts (PagerDuty, Slack)
4. 🔐 Set up RBAC and network policies
5. 🌍 Configure Ingress with TLS
6. 📈 Enable autoscaling (HPA)

---

## Resources

- **Workflow Details:** `.github/workflows/README.md`
- **Deployment Options:** `DEPLOYMENT.md`
- **Secrets Setup:** `GITHUB_SECRETS.md`
- **Vendoring Guide:** `VENDORING.md`

---

## Getting Help

1. Check workflow logs in GitHub Actions
2. Review deployment docs in `DEPLOYMENT.md`
3. Check troubleshooting section above
4. Review Kubernetes events: `kubectl get events -n default --sort-by='.lastTimestamp'`

Happy deploying! 🚀
