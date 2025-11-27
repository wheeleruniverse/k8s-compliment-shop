# GitHub Actions Workflows

This directory contains GitHub Actions workflows for CI/CD automation.

## Workflows

### 1. `build-and-deploy.yaml` - Build, Push, and Deploy

**Purpose:** Automatically builds Docker images, pushes them to GCP Artifact Registry with commit SHA tags, updates Helm values, and deploys to Kubernetes.

**Triggers:**
- Push to `main` branch (when services or Helm charts change)
- Manual trigger via `workflow_dispatch`

**What it does:**

1. **Detect Changes** - Identifies which services changed
2. **Build & Push** - Builds Docker images and pushes to Artifact Registry
   - Tags: `<commit-sha>` and `latest`
   - Example: `abc123f` and `latest`
3. **Update Helm Values** - Updates `k8s/helm/values.yaml` with new image tags
4. **Vendor Manifests** - Re-renders manifests to `k8s/rendered/`
5. **Commit Changes** - Commits updated values and manifests back to repo
6. **Deploy** - Deploys to your GKE cluster using Helm

**Secrets used:**
- `GCP_SERVICE_ACCOUNT_KEY` - Authenticate to GCP Artifact Registry
- `MYSQL_ROOT_PASSWORD` - MySQL password for deployment

**Variables used:**
- `GCP_REGION` - GCP region (us-central1)
- `GCP_PROJECT_ID` - GCP project ID (t-scarab-471016-n5)
- `GCP_ARTIFACT_REGISTRY_REPO` - Artifact Registry repo name (k8s-compliment-shop)

**Manual Trigger:**
```bash
# Via GitHub CLI
gh workflow run build-and-deploy.yaml

# Or with specific service
gh workflow run build-and-deploy.yaml -f service=web-service
```

**Configuration Required:**

Before the workflow can deploy, you need to configure your GKE cluster:

1. Open `.github/workflows/build-and-deploy.yaml`
2. Find the "Get GKE credentials" step (line ~234)
3. Replace `YOUR_CLUSTER_NAME` with your actual cluster name
4. Uncomment the `gcloud container clusters get-credentials` command
5. Uncomment the `helm upgrade` command in the "Deploy with Helm" step

Example:
```yaml
- name: Get GKE credentials
  run: |
    gcloud container clusters get-credentials my-k8s-cluster \
      --region ${{ vars.GCP_REGION }} \
      --project ${{ vars.GCP_PROJECT_ID }}
```

---

### 2. `vendor-manifests.yaml` - Vendor Kubernetes Manifests

**Purpose:** Automatically vendors (renders) Kubernetes manifests when Helm charts change.

**Triggers:**
- Pull requests that modify `k8s/helm/**` or vendoring script
- Push to `main` branch
- Manual trigger via `workflow_dispatch`

**What it does:**

1. Renders Helm + Kustomize templates
2. Outputs to `k8s/rendered/`
3. Commits changes if manifests changed
4. Adds PR comment on pull requests

**Secrets used:**
- `GITHUB_TOKEN` - Auto-provided for commits

---

## Docker Image Tagging Strategy

### Immutable Tags with Commit SHA

All Docker images are tagged with:
1. **Commit SHA (short)** - `abc123f` (first 7 chars)
2. **Latest** - `latest`

**Benefits:**
- ✅ Immutable - Each commit has a unique, permanent tag
- ✅ Traceable - Easy to identify which code is running
- ✅ Rollback-friendly - Can deploy any previous commit SHA
- ✅ Audit trail - Clear history of deployments

**Example:**
```
us-central1-docker.pkg.dev/t-scarab-471016-n5/k8s-compliment-shop/web-service:abc123f
us-central1-docker.pkg.dev/t-scarab-471016-n5/k8s-compliment-shop/web-service:latest
```

### Finding Image for a Specific Commit

```bash
# Get short SHA from commit
SHORT_SHA=$(git rev-parse --short=7 HEAD)

# Pull the image
docker pull us-central1-docker.pkg.dev/t-scarab-471016-n5/k8s-compliment-shop/web-service:$SHORT_SHA
```

---

## Workflow Outputs

### Build Summary

Each workflow run creates a summary showing:
- 🐳 Built images with tags
- 📦 Deployment status
- ⏰ Timestamp
- 🔗 Links to commits

Access via: Actions → Workflow run → Summary tab

---

## Service Change Detection

The workflow intelligently detects which services changed:

```yaml
services/web-service/**       → Builds web-service only
services/bff-service/**       → Builds bff-service only
services/product-service/**   → Builds product-service only
k8s/helm/**                   → Rebuilds all services
```

This saves time and resources by only building what changed.

---

## Local Testing

### Test the update script locally:

```bash
# Update a single service
./scripts/update-image-tags.sh web-service abc123f

# Update all services
./scripts/update-image-tags.sh all abc123f

# Verify changes
git diff k8s/helm/values.yaml
```

### Manually trigger vendoring:

```bash
./scripts/vendor-manifests.sh
```

### Build and push images manually:

```bash
# Authenticate to Artifact Registry
gcloud auth configure-docker us-central1-docker.pkg.dev

# Build and push
cd services/web-service
SHORT_SHA=$(git rev-parse --short=7 HEAD)
IMAGE=us-central1-docker.pkg.dev/t-scarab-471016-n5/k8s-compliment-shop/web-service

docker build -t $IMAGE:$SHORT_SHA -t $IMAGE:latest .
docker push $IMAGE:$SHORT_SHA
docker push $IMAGE:latest
```

---

## Troubleshooting

### Workflow fails on "Update image tags"

**Issue:** Script can't find or update values.yaml

**Solution:**
```bash
# Ensure script is executable
chmod +x scripts/update-image-tags.sh

# Test locally
./scripts/update-image-tags.sh web-service test123
git diff k8s/helm/values.yaml
```

### Workflow fails on Docker build

**Issue:** Docker build context or Dockerfile errors

**Solution:**
```bash
# Test build locally
cd services/web-service
docker build -t test .

# Check Dockerfile exists
ls -la Dockerfile
```

### Workflow fails on GCP authentication

**Issue:** `GCP_SERVICE_ACCOUNT_KEY` is invalid or missing

**Solution:**
1. Verify secret exists: GitHub → Settings → Secrets → Actions
2. Regenerate service account key if needed
3. Ensure service account has `roles/artifactregistry.writer` role

### Workflow fails on Helm deploy

**Issue:** Cluster credentials not configured or invalid

**Solution:**
1. Ensure you've uncommented and configured the GKE cluster name
2. Verify service account has GKE access
3. Test locally:
   ```bash
   gcloud container clusters get-credentials YOUR_CLUSTER_NAME \
     --region us-central1 \
     --project t-scarab-471016-n5

   kubectl get pods
   ```

---

## Best Practices

1. ✅ **Always vendor manifests** - The workflow automatically vendors, but run locally before pushing
2. ✅ **Review rendered diffs** - Check what actually changes in `k8s/rendered/`
3. ✅ **Use semantic commit messages** - Helps track what changed in deployments
4. ✅ **Monitor workflow runs** - Check Actions tab for failures
5. ✅ **Tag releases** - Create Git tags for major deployments
6. ✅ **Test locally first** - Build and test images before pushing

---

## Environment Setup Checklist

- [x] `GCP_SERVICE_ACCOUNT_KEY` secret configured
- [x] `MYSQL_ROOT_PASSWORD` secret configured
- [x] `GCP_PROJECT_ID` variable configured
- [x] `GCP_REGION` variable configured
- [x] `GCP_ARTIFACT_REGISTRY_REPO` variable configured
- [ ] GKE cluster name configured in workflow
- [ ] Helm deploy command uncommented
- [ ] Test deployment successful

---

## Advanced Usage

### Deploy specific service manually

```bash
# Via GitHub UI
# Actions → build-and-deploy → Run workflow → Select service

# Via GitHub CLI
gh workflow run build-and-deploy.yaml -f service=web-service
```

### Rollback to previous commit

```bash
# Find previous commit SHA
git log --oneline

# Get short SHA (first 7 chars)
# Update values.yaml with that SHA
./scripts/update-image-tags.sh web-service abc1234

# Vendor and commit
./scripts/vendor-manifests.sh
git add k8s/
git commit -m "chore: rollback web-service to abc1234"
git push

# Workflow will deploy the old image
```

### Deploy from a different branch

```bash
# Modify workflow trigger to include your branch
# .github/workflows/build-and-deploy.yaml
on:
  push:
    branches:
      - main
      - staging  # Add this

# Push to staging branch to deploy
git checkout -b staging
git push origin staging
```

---

## Monitoring

### View deployment history

```bash
# GitHub CLI
gh run list --workflow=build-and-deploy.yaml

# See details
gh run view <run-id>
```

### Check deployed images

```bash
# Get current image tags from cluster
kubectl get deployment web-service -n default -o jsonpath='{.spec.template.spec.containers[0].image}'
```

### Audit trail

All deployments are:
- ✅ Tracked in Git commits
- ✅ Logged in Actions workflow runs
- ✅ Visible in Helm release history
- ✅ Tied to specific commit SHAs

```bash
# View Helm release history
helm history k8s-compliment-shop -n default
```
