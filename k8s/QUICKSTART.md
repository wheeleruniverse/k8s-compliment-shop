# Quick Start Guide

Get up and running with the k8s-compliment-shop Helm chart in 5 minutes.

## Prerequisites Installation

### macOS

```bash
# Install Helm
brew install helm

# Install Kustomize
brew install kustomize

# Install kubectl (if not already installed)
brew install kubectl

# Install ArgoCD CLI (optional, for ArgoCD management)
brew install argocd
```

### Linux

```bash
# Install Helm
curl https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | bash

# Install Kustomize
curl -s "https://raw.githubusercontent.com/kubernetes-sigs/kustomize/master/hack/install_kustomize.sh" | bash
sudo mv kustomize /usr/local/bin/

# Install kubectl
curl -LO "https://dl.k8s.io/release/$(curl -L -s https://dl.k8s.io/release/stable.txt)/bin/linux/amd64/kubectl"
chmod +x kubectl
sudo mv kubectl /usr/local/bin/

# Install ArgoCD CLI (optional)
curl -sSL -o argocd https://github.com/argoproj/argo-cd/releases/latest/download/argocd-linux-amd64
chmod +x argocd
sudo mv argocd /usr/local/bin/
```

### Windows (PowerShell)

```powershell
# Install Helm
choco install kubernetes-helm

# Install Kustomize
choco install kustomize

# Install kubectl
choco install kubernetes-cli

# Install ArgoCD CLI
choco install argocd-cli
```

## 1. Local Development (Test without ArgoCD)

### Render Manifests Locally

```bash
# Navigate to project root
cd k8s-compliment-shop

# Render with Helm + Kustomize
./scripts/vendor-manifests.sh

# View rendered manifests
cat k8s/rendered/all-resources.yaml
```

### Validate Manifests

```bash
# Lint Helm chart
helm lint k8s/helm

# Dry-run to check for errors
kubectl apply --dry-run=client -f k8s/rendered/all-resources.yaml
```

### Deploy to Local Cluster

```bash
# Using rendered manifests
kubectl apply -f k8s/rendered/all-resources.yaml

# Or using Helm directly
helm install k8s-compliment-shop k8s/helm \
  --namespace default \
  --post-renderer k8s/helm/kustomize/post-renderer.sh

# Check deployment
kubectl get pods
kubectl get services
```

### Access the Application

```bash
# Port forward to web service
kubectl port-forward svc/web-service 8080:8080

# Open http://localhost:8080
```

### Cleanup

```bash
# If installed via kubectl
kubectl delete -f k8s/rendered/all-resources.yaml

# If installed via Helm
helm uninstall k8s-compliment-shop
```

## 2. Production Deployment (with ArgoCD)

### Install ArgoCD

```bash
# Create ArgoCD namespace
kubectl create namespace argocd

# Install ArgoCD
kubectl apply -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml

# Wait for ArgoCD to be ready
kubectl wait --for=condition=available --timeout=600s deployment/argocd-server -n argocd
```

### Configure ArgoCD

```bash
# Get admin password
kubectl -n argocd get secret argocd-initial-admin-secret -o jsonpath="{.data.password}" | base64 -d && echo

# Port forward to ArgoCD UI
kubectl port-forward svc/argocd-server -n argocd 8080:443

# Login (username: admin, password: from above)
# Open https://localhost:8080
```

### Update Repository URL

Edit `k8s/argocd/application.yaml`:
```yaml
spec:
  source:
    repoURL: https://github.com/YOUR-USERNAME/k8s-compliment-shop.git  # Update this line!
```

Commit and push:
```bash
git add k8s/argocd/application.yaml
git commit -m "chore: update ArgoCD repository URL"
git push
```

### Deploy Application

```bash
# Apply ArgoCD Application
kubectl apply -f k8s/argocd/application.yaml

# Check status
kubectl get application -n argocd

# Sync application (if not auto-syncing)
argocd app sync k8s-compliment-shop
```

### Monitor Deployment

```bash
# Via CLI
argocd app get k8s-compliment-shop

# Via UI
# Open https://localhost:8080
# Navigate to Applications > k8s-compliment-shop
```

## 3. Making Changes

### Update Image Version

1. Edit `k8s/helm/values.yaml`:
   ```yaml
   webService:
     image:
       tag: v7  # Change from v6 to v7
   ```

2. Commit and push:
   ```bash
   git add k8s/helm/values.yaml
   git commit -m "feat: update web-service to v7"
   git push
   ```

3. GitHub Actions will:
   - Render manifests
   - Commit to `k8s/rendered/`

4. ArgoCD will:
   - Detect change
   - Auto-sync (if enabled)
   - Update cluster

### Scale Replicas

1. Edit `k8s/helm/values.yaml`:
   ```yaml
   webService:
     replicaCount: 5  # Change from 2 to 5
   ```

2. Commit, push, and wait for auto-sync

### Update Configuration

1. Edit `k8s/helm/values.yaml`:
   ```yaml
   webService:
     config:
       gaMeasurementId: "G-NEWID456"
   ```

2. Commit and push

## 4. Useful Commands

### Helm Commands

```bash
# List installed releases
helm list

# Get release status
helm status k8s-compliment-shop

# Upgrade release
helm upgrade k8s-compliment-shop k8s/helm --post-renderer k8s/helm/kustomize/post-renderer.sh

# Rollback to previous version
helm rollback k8s-compliment-shop

# Uninstall release
helm uninstall k8s-compliment-shop
```

### ArgoCD Commands

```bash
# List applications
argocd app list

# Get application details
argocd app get k8s-compliment-shop

# Sync application
argocd app sync k8s-compliment-shop

# View logs
argocd app logs k8s-compliment-shop

# Diff local vs cluster
argocd app diff k8s-compliment-shop

# Rollback to previous version
argocd app rollback k8s-compliment-shop
```

### Kubernetes Commands

```bash
# Get all resources
kubectl get all

# Get pods
kubectl get pods

# View logs
kubectl logs -l app=web-service
kubectl logs -l app=bff-service
kubectl logs -l app=product-service

# Describe resource
kubectl describe deployment web-service

# Port forward
kubectl port-forward svc/web-service 8080:8080

# Execute command in pod
kubectl exec -it <pod-name> -- /bin/bash
```

## 5. Troubleshooting

### Helm template fails

```bash
# Lint chart
helm lint k8s/helm

# Debug template rendering
helm template k8s-compliment-shop k8s/helm --debug
```

### Kustomize post-renderer fails

```bash
# Test kustomize separately
helm template k8s-compliment-shop k8s/helm > /tmp/test.yaml
cat /tmp/test.yaml | kustomize build k8s/helm/kustomize
```

### ArgoCD not syncing

```bash
# Check application status
argocd app get k8s-compliment-shop

# Force sync
argocd app sync k8s-compliment-shop --prune --force

# Check ArgoCD logs
kubectl logs -n argocd -l app.kubernetes.io/name=argocd-application-controller
```

### Pods not starting

```bash
# Check pod status
kubectl get pods

# View pod events
kubectl describe pod <pod-name>

# View pod logs
kubectl logs <pod-name>

# Check resource limits
kubectl top pods
```

## Next Steps

1. Read `k8s/README.md` for comprehensive documentation
2. Review `k8s/MIGRATION.md` if migrating from old structure
3. Check `k8s/argocd/README.md` for ArgoCD details
4. Explore `k8s/helm/kustomize/README.md` for Kustomize usage

## Support

- **Documentation**: See `k8s/README.md`
- **ArgoCD Help**: See `k8s/argocd/README.md`
- **Kustomize Help**: See `k8s/helm/kustomize/README.md`
