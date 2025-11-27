# Deployment Guide

This guide explains how to deploy the k8s-compliment-shop application to your Kubernetes cluster using the configured GitHub secrets.

## Prerequisites

- ✅ Kubernetes cluster (GKE, minikube, etc.)
- ✅ `kubectl` configured to access your cluster
- ✅ `helm` installed (v3+)
- ✅ GitHub secrets configured (see `GITHUB_SECRETS.md`)

---

## Deployment Options

### Option 1: Deploy with Helm Using GitHub Secrets (Recommended)

This approach uses the GitHub secrets you've configured for a secure deployment.

#### Step 1: Authenticate to GCP Artifact Registry

```bash
# Authenticate using your GCP service account
echo "$GCP_SERVICE_ACCOUNT_KEY" | docker login -u _json_key --password-stdin \
  https://us-central1-docker.pkg.dev

# Or if you're using gcloud CLI
gcloud auth configure-docker us-central1-docker.pkg.dev
```

#### Step 2: Deploy with Helm

```bash
# Set environment variables (these would come from GitHub secrets in CI/CD)
export MYSQL_ROOT_PASSWORD="your-secure-password"
export GCP_PROJECT_ID="t-scarab-471016-n5"
export GCP_REGION="us-central1"

# Deploy using Helm
helm upgrade --install k8s-compliment-shop k8s/helm \
  --set mysql.auth.rootPassword="$MYSQL_ROOT_PASSWORD" \
  --namespace default \
  --create-namespace \
  --wait
```

#### Step 3: Verify Deployment

```bash
# Check pods
kubectl get pods -n default

# Check services
kubectl get services -n default

# Get the LoadBalancer IP (for web-service)
kubectl get service web-service -n default -o jsonpath='{.status.loadBalancer.ingress[0].ip}'
```

---

### Option 2: Deploy with values.prod.yaml (Local Development)

For local development, you can create a `values.prod.yaml` file (which is gitignored).

#### Step 1: Create values.prod.yaml

```bash
# Create the production values file (DO NOT COMMIT THIS!)
cat > k8s/helm/values.prod.yaml <<EOF
mysql:
  auth:
    rootPassword: "your-secure-mysql-password"
EOF
```

#### Step 2: Deploy

```bash
# Deploy using both values files
helm upgrade --install k8s-compliment-shop k8s/helm \
  --values k8s/helm/values.yaml \
  --values k8s/helm/values.prod.yaml \
  --namespace default \
  --create-namespace \
  --wait
```

---

### Option 3: Deploy Rendered Manifests with kubectl (GitOps Style)

This uses the vendored manifests from `k8s/rendered/` and applies secrets separately.

#### Step 1: Create Secret Manually

```bash
# Create MySQL secret
kubectl create secret generic mysql-secret \
  --from-literal=root-password="your-secure-password" \
  --from-literal=connection-string="Server=mysql-service;Port=3306;Database=complimentshop;User=root;Password=your-secure-password;" \
  --namespace default \
  --dry-run=client -o yaml | kubectl apply -f -
```

#### Step 2: Apply Rendered Manifests

```bash
# Apply the vendored manifests (excluding secrets since we created it above)
kubectl apply -f k8s/rendered/deployments/
kubectl apply -f k8s/rendered/services/
kubectl apply -f k8s/rendered/statefulsets/
kubectl apply -f k8s/rendered/configmaps/

# Or apply all at once (will use the manually created secret)
kubectl apply -f k8s/rendered/all-resources.yaml
```

---

## CI/CD Deployment (GitHub Actions Example)

Here's how you would deploy using GitHub Actions with your configured secrets:

```yaml
name: Deploy to GKE

on:
  push:
    branches:
      - main
  workflow_dispatch:

jobs:
  deploy:
    runs-on: ubuntu-latest

    permissions:
      contents: read
      id-token: write

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Authenticate to Google Cloud
        uses: google-github-actions/auth@v2
        with:
          credentials_json: ${{ secrets.GCP_SERVICE_ACCOUNT_KEY }}

      - name: Set up Cloud SDK
        uses: google-github-actions/setup-gcloud@v2

      - name: Configure Docker for Artifact Registry
        run: |
          gcloud auth configure-docker ${{ vars.GCP_REGION }}-docker.pkg.dev

      - name: Get GKE credentials
        run: |
          gcloud container clusters get-credentials YOUR_CLUSTER_NAME \
            --region ${{ vars.GCP_REGION }} \
            --project ${{ vars.GCP_PROJECT_ID }}

      - name: Set up Helm
        uses: azure/setup-helm@v4
        with:
          version: 'latest'

      - name: Deploy with Helm
        run: |
          helm upgrade --install k8s-compliment-shop k8s/helm \
            --set mysql.auth.rootPassword="${{ secrets.MYSQL_ROOT_PASSWORD }}" \
            --namespace default \
            --create-namespace \
            --wait \
            --timeout 10m

      - name: Verify Deployment
        run: |
          kubectl get pods -n default
          kubectl get services -n default
```

---

## Environment-Specific Deployments

### Development

```bash
helm upgrade --install k8s-compliment-shop k8s/helm \
  --set mysql.auth.rootPassword="dev-password" \
  --set webService.replicaCount=1 \
  --set bffService.replicaCount=1 \
  --set productService.replicaCount=1 \
  --namespace dev \
  --create-namespace
```

### Staging

```bash
helm upgrade --install k8s-compliment-shop k8s/helm \
  --set mysql.auth.rootPassword="${{ secrets.MYSQL_ROOT_PASSWORD_STAGING }}" \
  --namespace staging \
  --create-namespace
```

### Production

```bash
helm upgrade --install k8s-compliment-shop k8s/helm \
  --set mysql.auth.rootPassword="${{ secrets.MYSQL_ROOT_PASSWORD }}" \
  --set webService.replicaCount=3 \
  --set bffService.replicaCount=3 \
  --set productService.replicaCount=3 \
  --namespace production \
  --create-namespace
```

---

## ArgoCD Deployment

If you're using ArgoCD for GitOps:

### Step 1: Create ArgoCD Application

```yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: k8s-compliment-shop
  namespace: argocd
spec:
  project: default

  source:
    repoURL: https://github.com/YOUR_USERNAME/k8s-compliment-shop.git
    targetRevision: main
    path: k8s/helm
    helm:
      values: |
        mysql:
          auth:
            rootPassword: "OVERRIDE_WITH_SEALED_SECRET"

  destination:
    server: https://kubernetes.default.svc
    namespace: default

  syncPolicy:
    automated:
      prune: true
      selfHeal: true
    syncOptions:
      - CreateNamespace=true
```

### Step 2: Use Sealed Secrets for MySQL Password

```bash
# Install Sealed Secrets controller
kubectl apply -f https://github.com/bitnami-labs/sealed-secrets/releases/download/v0.24.0/controller.yaml

# Create a sealed secret for MySQL
echo -n "your-secure-password" | kubectl create secret generic mysql-secret \
  --dry-run=client \
  --from-file=root-password=/dev/stdin \
  -o yaml | \
  kubeseal -o yaml > k8s/sealed-secrets/mysql-secret-sealed.yaml

# Commit the sealed secret (it's encrypted, safe to commit)
git add k8s/sealed-secrets/mysql-secret-sealed.yaml
git commit -m "Add sealed secret for MySQL"
git push
```

---

## Post-Deployment Verification

### Check Application Health

```bash
# Check all pods are running
kubectl get pods -n default

# Check logs for web-service
kubectl logs -n default -l app=web-service --tail=50

# Check logs for bff-service
kubectl logs -n default -l app=bff-service --tail=50

# Check logs for product-service
kubectl logs -n default -l app=product-service --tail=50

# Check MySQL
kubectl logs -n default -l app=mysql --tail=50
```

### Test the Application

```bash
# Get the LoadBalancer IP
export WEB_SERVICE_IP=$(kubectl get service web-service -n default -o jsonpath='{.status.loadBalancer.ingress[0].ip}')

# Test the application
curl http://$WEB_SERVICE_IP:8080/health

# Open in browser
echo "Open: http://$WEB_SERVICE_IP:8080"
```

### Port Forward for Local Testing

```bash
# Port forward web service
kubectl port-forward -n default service/web-service 8080:8080

# Open browser to http://localhost:8080
```

---

## Rollback

If you need to rollback a deployment:

```bash
# List Helm releases
helm list -n default

# Rollback to previous version
helm rollback k8s-compliment-shop -n default

# Or rollback to specific revision
helm rollback k8s-compliment-shop 2 -n default
```

---

## Cleanup

To completely remove the deployment:

```bash
# Uninstall with Helm
helm uninstall k8s-compliment-shop -n default

# Delete persistent volumes (if needed)
kubectl delete pvc -n default -l app=mysql

# Delete namespace (if desired)
kubectl delete namespace default
```

---

## Troubleshooting

### Pods Not Starting

```bash
# Describe pod to see events
kubectl describe pod <pod-name> -n default

# Check logs
kubectl logs <pod-name> -n default

# Check if images can be pulled
kubectl get events -n default --sort-by='.lastTimestamp'
```

### Image Pull Errors

```bash
# Verify Artifact Registry authentication
gcloud auth configure-docker us-central1-docker.pkg.dev

# Create image pull secret for the cluster
kubectl create secret docker-registry gcr-json-key \
  --docker-server=us-central1-docker.pkg.dev \
  --docker-username=_json_key \
  --docker-password="$(cat key.json)" \
  --namespace=default

# Add to service account
kubectl patch serviceaccount default -n default \
  -p '{"imagePullSecrets": [{"name": "gcr-json-key"}]}'
```

### MySQL Connection Issues

```bash
# Test MySQL connectivity from within a pod
kubectl run -it --rm debug --image=mysql:8.0 --restart=Never -- \
  mysql -h mysql-service -u root -p

# Check MySQL secret
kubectl get secret mysql-secret -n default -o yaml
```

---

## Security Best Practices

1. ✅ **Secrets configured in GitHub** - Never commit real passwords
2. ✅ **values.prod.yaml in .gitignore** - Local prod values won't be committed
3. ✅ **Use Sealed Secrets or External Secrets** - For ArgoCD deployments
4. ✅ **Rotate secrets regularly** - Update `MYSQL_ROOT_PASSWORD` every 90 days
5. ✅ **Use namespace isolation** - Separate dev/staging/prod namespaces
6. ✅ **Enable RBAC** - Restrict access to secrets and resources
7. ✅ **Audit access** - Monitor who deploys and when

---

## Next Steps

1. ✅ All secrets configured
2. 🔄 Choose your deployment method (Helm, kubectl, ArgoCD)
3. 🚀 Deploy to your cluster
4. ✅ Verify application is running
5. 📊 Set up monitoring (Prometheus, Grafana)
6. 🔔 Configure alerting
7. 📝 Document your specific deployment process

For questions or issues, refer to `GITHUB_SECRETS.md` for secret configuration details.
