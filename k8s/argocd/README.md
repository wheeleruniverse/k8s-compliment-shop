# ArgoCD Configuration

This directory contains ArgoCD Application manifests for deploying k8s-compliment-shop.

## Prerequisites

1. **ArgoCD installed** in your cluster
   ```bash
   kubectl create namespace argocd
   kubectl apply -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml
   ```

2. **Update repository URL** in `application.yaml`:
   ```yaml
   source:
     repoURL: https://github.com/YOUR-ORG/k8s-compliment-shop.git
   ```

## Deployment

### Apply the Application

```bash
kubectl apply -f k8s/argocd/application.yaml
```

### Access ArgoCD UI

```bash
# Get admin password
kubectl -n argocd get secret argocd-initial-admin-secret -o jsonpath="{.data.password}" | base64 -d

# Port forward to access UI
kubectl port-forward svc/argocd-server -n argocd 8080:443

# Open https://localhost:8080
# Username: admin
# Password: (from command above)
```

### Sync the Application

```bash
# Sync via CLI
argocd app sync k8s-compliment-shop

# Or use the UI
# Navigate to Applications > k8s-compliment-shop > Sync
```

## How It Works

1. **ArgoCD watches** the Git repository for changes
2. **Helm renders** templates using `values.yaml`
3. **Kustomize post-processes** the rendered manifests (adds labels, patches, etc.)
4. **ArgoCD applies** the final manifests to the cluster
5. **Continuous sync** ensures cluster state matches Git (GitOps!)

## Application Configuration

### Key Settings

- **Auto-sync**: Enabled - automatically deploys changes from Git
- **Self-heal**: Enabled - reverts manual cluster changes back to Git state
- **Prune**: Enabled - removes resources deleted from Git

### Customizing for Your Environment

Edit `application.yaml` to customize:

1. **Repository**:
   ```yaml
   source:
     repoURL: https://github.com/your-org/k8s-compliment-shop.git
     targetRevision: main  # or production, staging, etc.
   ```

2. **Namespace**:
   ```yaml
   destination:
     namespace: production  # change from default
   ```

3. **Override values**:
   ```yaml
   helm:
     values: |
       webService:
         image:
           tag: v7
         replicaCount: 3
   ```

## Using with Kustomize Post-Renderer

The application is configured to use Kustomize as a post-renderer:

```yaml
helm:
  kustomize:
    path: kustomize  # relative to helm chart directory
```

This allows you to:
- Add common labels/annotations via Kustomize
- Apply patches without modifying Helm templates
- Use Kustomize overlays for environment-specific changes

## Troubleshooting

### Check Application Status

```bash
argocd app get k8s-compliment-shop
```

### View Sync Logs

```bash
argocd app logs k8s-compliment-shop
```

### Manual Sync

```bash
argocd app sync k8s-compliment-shop --prune
```

### Delete and Recreate

```bash
argocd app delete k8s-compliment-shop --cascade
kubectl apply -f k8s/argocd/application.yaml
```

## Best Practices

1. **Use separate applications** for different environments (dev, staging, prod)
2. **Version your Helm chart** in `Chart.yaml`
3. **Use value files** for environment-specific configuration
4. **Enable notifications** for sync failures
5. **Review rendered manifests** in CI before ArgoCD applies them (vendoring)
