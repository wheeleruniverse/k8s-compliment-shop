# Migration Guide

This guide helps you migrate from the old scattered K8s manifests to the new centralized Helm + Kustomize + ArgoCD setup.

## What Changed

### Before (Old Structure)

```
services/
├── web-service/k8s/
│   ├── deployment.yaml
│   ├── service.yaml
│   └── configmap.yaml
├── bff-service/k8s/
│   ├── deployment.yaml
│   └── service.yaml
└── product-service/k8s/
    ├── deployment.yaml
    └── service.yaml
infrastructure/
└── mysql/
    ├── statefulset.yaml
    ├── service.yaml
    ├── configmap.yaml
    └── secret.yaml
```

**Problems:**
- Manifests scattered across multiple directories
- Hard to manage all services together
- No templating or reusability
- Manual application with kubectl
- Hard to see what changed before applying

### After (New Structure)

```
k8s/
├── helm/                    # Single source of truth
│   ├── Chart.yaml
│   ├── values.yaml         # All config in one place
│   ├── templates/          # All services
│   └── kustomize/          # Post-renderer patches
├── argocd/                 # GitOps deployment
│   └── application.yaml
└── rendered/               # Vendored for PR review
    └── all-resources.yaml
```

**Benefits:**
- All manifests in one place
- Templated with Helm (DRY principle)
- Configurable via values.yaml
- GitOps with ArgoCD (automated sync)
- PR review of rendered manifests (vendoring)
- Enterprise-ready structure

## Migration Steps

### Step 1: Verify New Helm Chart

```bash
# Render the new Helm chart
./scripts/vendor-manifests.sh

# Review rendered manifests
cat k8s/rendered/all-resources.yaml
```

### Step 2: Compare Old vs New

```bash
# Get old manifests
kubectl apply --dry-run=client -f services/web-service/k8s/ -o yaml > /tmp/old-web.yaml
kubectl apply --dry-run=client -f services/bff-service/k8s/ -o yaml > /tmp/old-bff.yaml
kubectl apply --dry-run=client -f services/product-service/k8s/ -o yaml > /tmp/old-product.yaml
kubectl apply --dry-run=client -f infrastructure/mysql/ -o yaml > /tmp/old-mysql.yaml

# Get new manifests
cat k8s/rendered/all-resources.yaml > /tmp/new-all.yaml

# Compare (should be mostly the same, minus Helm labels)
diff /tmp/old-web.yaml /tmp/new-all.yaml
```

### Step 3: Test Locally (Optional)

If you have a local cluster (minikube, kind, etc.):

```bash
# Install with new Helm chart
helm install k8s-compliment-shop k8s/helm \
  --namespace default \
  --post-renderer k8s/helm/kustomize/post-renderer.sh

# Verify all resources are created
kubectl get all

# Test the application
kubectl port-forward svc/web-service 8080:8080
```

### Step 4: Deploy to Production with ArgoCD

#### 4.1 Install ArgoCD

```bash
kubectl create namespace argocd
kubectl apply -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml
```

#### 4.2 Update Repository URL

Edit `k8s/argocd/application.yaml`:
```yaml
source:
  repoURL: https://github.com/YOUR-ORG/k8s-compliment-shop.git
```

#### 4.3 Apply ArgoCD Application

```bash
kubectl apply -f k8s/argocd/application.yaml
```

#### 4.4 Sync the Application

```bash
# Via CLI
argocd app sync k8s-compliment-shop

# Or via UI
kubectl port-forward svc/argocd-server -n argocd 8080:443
# Open https://localhost:8080 and click "Sync"
```

### Step 5: Verify Deployment

```bash
# Check ArgoCD application status
kubectl get application -n argocd k8s-compliment-shop
argocd app get k8s-compliment-shop

# Check all resources
kubectl get all

# Check if services are healthy
kubectl get pods
kubectl logs -l app=web-service
kubectl logs -l app=bff-service
kubectl logs -l app=product-service
```

### Step 6: Clean Up Old Manifests

Once everything is working:

```bash
# Remove old K8s manifests (DO NOT do this until verified!)
git rm -r services/web-service/k8s/
git rm -r services/bff-service/k8s/
git rm -r services/product-service/k8s/
git rm -r infrastructure/mysql/

git commit -m "chore: remove old K8s manifests, migrated to Helm chart"
git push
```

## Rolling Back

If you need to rollback to the old manifests:

```bash
# Delete ArgoCD application
kubectl delete -f k8s/argocd/application.yaml

# Delete Helm release
helm uninstall k8s-compliment-shop

# Reapply old manifests
kubectl apply -f services/web-service/k8s/
kubectl apply -f services/bff-service/k8s/
kubectl apply -f services/product-service/k8s/
kubectl apply -f infrastructure/mysql/
```

## Differences to Be Aware Of

### 1. Helm Adds Labels

The new Helm chart adds standard labels:
```yaml
labels:
  helm.sh/chart: k8s-compliment-shop-1.0.0
  app.kubernetes.io/name: k8s-compliment-shop
  app.kubernetes.io/instance: k8s-compliment-shop
  app.kubernetes.io/managed-by: Helm
```

These are harmless and help with resource management.

### 2. Kustomize Adds Labels (if configured)

The Kustomize post-renderer adds:
```yaml
labels:
  app.kubernetes.io/part-of: k8s-compliment-shop
  environment: production
annotations:
  managed-by: argocd
```

### 3. Resource Names Stay the Same

All resource names (web-service, bff-service, etc.) remain unchanged, so existing services will be updated in-place.

### 4. Secrets and ConfigMaps

Secrets and ConfigMaps are now managed by Helm. If you're using external secret management (like Google Secret Manager), you may want to integrate that with Helm.

## FAQ

### Q: Can I still use kubectl apply?

Yes, but it's not recommended. ArgoCD provides GitOps workflow which is better for production:
- All changes go through Git
- Automatic sync
- Rollback via Git
- Audit trail

### Q: What if I need to make a quick fix?

1. Edit `k8s/helm/values.yaml`
2. Commit and push
3. ArgoCD syncs automatically
4. Or force sync: `argocd app sync k8s-compliment-shop`

### Q: How do I update image versions?

Edit `k8s/helm/values.yaml`:
```yaml
webService:
  image:
    tag: v7  # Change this
```

### Q: Can I disable auto-sync?

Yes, edit `k8s/argocd/application.yaml`:
```yaml
syncPolicy:
  automated: null  # Remove automated section
```

Then sync manually when ready.

### Q: What happens to existing pods?

When you apply the Helm chart, Kubernetes will update existing resources in-place. Pods will be rolling-updated according to their deployment strategy.

### Q: How do I test changes before deploying?

Use the vendoring workflow:
1. Make changes to `values.yaml`
2. Push to a branch and open PR
3. GitHub Actions renders manifests
4. Review `k8s/rendered/all-resources.yaml` in PR
5. Merge when ready

## Troubleshooting

### Issue: ArgoCD shows "OutOfSync"

**Cause:** Cluster state doesn't match Git state.

**Solution:**
```bash
argocd app sync k8s-compliment-shop --prune
```

### Issue: Helm template error

**Cause:** Syntax error in templates or values.

**Solution:**
```bash
helm lint k8s/helm
helm template k8s-compliment-shop k8s/helm --debug
```

### Issue: Kustomize post-renderer fails

**Cause:** Invalid kustomization.yaml or missing kustomize binary.

**Solution:**
```bash
# Test kustomize separately
helm template k8s-compliment-shop k8s/helm > /tmp/helm-output.yaml
cat /tmp/helm-output.yaml | kustomize build k8s/helm/kustomize
```

### Issue: Resources stuck in "Terminating"

**Cause:** Finalizers or dependent resources.

**Solution:**
```bash
# Remove finalizers
kubectl patch <resource> <name> -p '{"metadata":{"finalizers":[]}}' --type=merge

# Or wait for dependencies to be cleaned up
```

## Getting Help

1. Check `k8s/README.md` for general usage
2. Check `k8s/argocd/README.md` for ArgoCD-specific help
3. Review rendered manifests in `k8s/rendered/`
4. Use `helm template --debug` to troubleshoot
