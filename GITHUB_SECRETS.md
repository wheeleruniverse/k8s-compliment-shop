# GitHub Repository Secrets & Variables Setup

This document outlines the GitHub secrets and variables you need to configure for your k8s-compliment-shop repository.

## Current Status ✅

**Secrets configured:**
- ✅ `GCP_SERVICE_ACCOUNT_KEY` - Service account for GCP Artifact Registry
- ✅ `MYSQL_ROOT_PASSWORD` - MySQL root password for production

**Variables configured:**
- ✅ `GCP_REGION` - us-central1
- ✅ `GCP_PROJECT_ID` - t-scarab-471016-n5
- ✅ `GCP_ARTIFACT_REGISTRY_REPO` - k8s-compliment-shop

**Status:** All required secrets and variables are configured! 🎉

---

## Required Secrets

### 1. Database Secrets

#### `MYSQL_ROOT_PASSWORD`
- **Type:** Repository Secret
- **Purpose:** MySQL root password for production deployments
- **Used by:** Helm deployment (override placeholder value)
- **Where it appears:**
  - `k8s/helm/values.yaml` (hardcoded as `"yourpassword"` - placeholder)
  - `k8s/helm/values.public.yaml` (uses `PLACEHOLDER_REPLACE_AT_DEPLOY_TIME`)
  - Referenced in MySQL StatefulSet for `MYSQL_ROOT_PASSWORD` env var
  - Referenced in MySQL readiness probe

**How to set:**
```bash
# In GitHub UI: Settings → Secrets and variables → Actions → New repository secret
# Name: MYSQL_ROOT_PASSWORD
# Value: <your-secure-mysql-password>
```

**How it's used in deployment:**
```bash
helm upgrade --install k8s-compliment-shop k8s/helm \
  --set mysql.auth.rootPassword="${{ secrets.MYSQL_ROOT_PASSWORD }}" \
  --namespace default
```

---

## Optional/Future Secrets

### 2. GCP/Artifact Registry Authentication (if using private registries)

Your images are hosted at:
- `us-central1-docker.pkg.dev/t-scarab-471016-n5/k8s-compliment-shop/*`

If your Artifact Registry is **private**, you'll need:

#### `GCP_PROJECT_ID`
- **Type:** Repository Variable (not secret, it's public in your manifests)
- **Value:** `t-scarab-471016-n5`
- **Purpose:** Reference your GCP project ID in workflows

#### `GCP_SERVICE_ACCOUNT_KEY`
- **Type:** Repository Secret
- **Purpose:** Authenticate to Google Artifact Registry for pushing/pulling images
- **When needed:** If you add CI/CD workflows that build and push Docker images

**How to create:**
```bash
# Create service account
gcloud iam service-accounts create github-actions \
  --project=t-scarab-471016-n5

# Grant Artifact Registry Writer role
gcloud projects add-iam-policy-binding t-scarab-471016-n5 \
  --member="serviceAccount:github-actions@t-scarab-471016-n5.iam.gserviceaccount.com" \
  --role="roles/artifactregistry.writer"

# Create and download key
gcloud iam service-accounts keys create key.json \
  --iam-account=github-actions@t-scarab-471016-n5.iam.gserviceaccount.com

# Copy the contents of key.json to GitHub secret GCP_SERVICE_ACCOUNT_KEY
# Then delete key.json locally!
```

---

## Repository Variables (Non-Sensitive)

These can be set as **Variables** instead of secrets since they're not sensitive:

### `GCP_PROJECT_ID`
- **Value:** `t-scarab-471016-n5`
- **Purpose:** Reference in workflows without hardcoding

### `GCP_REGION`
- **Value:** `us-central1`
- **Purpose:** Reference the region for Artifact Registry

### `ARTIFACT_REGISTRY_REPO`
- **Value:** `k8s-compliment-shop`
- **Purpose:** Reference the Artifact Registry repository name

**How to set variables:**
```
GitHub UI: Settings → Secrets and variables → Actions → Variables tab → New repository variable
```

---

## Current Workflow Status

### `vendor-manifests.yaml`
**Secrets used:**
- ✅ `secrets.GITHUB_TOKEN` - Automatically provided by GitHub Actions (no setup needed)

**Analysis:** This workflow only vendors manifests and doesn't deploy, so it doesn't need MySQL password or GCP credentials.

---

## Secrets Summary Table

| Secret/Variable Name | Type | Status | Purpose |
|------------|------|--------|---------|
| `GITHUB_TOKEN` | Auto-provided | ✅ Active | GitHub Actions automation |
| `MYSQL_ROOT_PASSWORD` | Secret | ✅ Configured | MySQL database password |
| `GCP_SERVICE_ACCOUNT_KEY` | Secret | ✅ Configured | Push/pull images to Artifact Registry |
| `GCP_PROJECT_ID` | Variable | ✅ Configured | Reference GCP project (t-scarab-471016-n5) |
| `GCP_REGION` | Variable | ✅ Configured | Reference GCP region (us-central1) |
| `GCP_ARTIFACT_REGISTRY_REPO` | Variable | ✅ Configured | Artifact Registry repo name (k8s-compliment-shop) |

---

## Next Steps

### Immediate (Required for Deployment)

1. **Set `MYSQL_ROOT_PASSWORD` secret:**
   - Go to: `https://github.com/<your-username>/k8s-compliment-shop/settings/secrets/actions`
   - Click "New repository secret"
   - Name: `MYSQL_ROOT_PASSWORD`
   - Value: Generate a strong password (e.g., `openssl rand -base64 32`)
   - Click "Add secret"

2. **Update your deployment commands** to use the secret:
   ```bash
   # When deploying via Helm
   helm upgrade --install k8s-compliment-shop k8s/helm \
     --set mysql.auth.rootPassword="$MYSQL_ROOT_PASSWORD" \
     --namespace default
   ```

3. **Create `values.prod.yaml` locally** (DO NOT COMMIT):
   ```yaml
   mysql:
     auth:
       rootPassword: "<your-actual-password>"
   ```

   Add to `.gitignore`:
   ```
   k8s/helm/values.prod.yaml
   ```

### Future (When Adding CI/CD)

When you add workflows to build and push Docker images:

1. Set up `GCP_SERVICE_ACCOUNT_KEY` secret (see instructions above)
2. Set up `GCP_PROJECT_ID` variable
3. Add Docker build/push workflows that authenticate to GCP

---

## Security Best Practices

1. ✅ **Never commit secrets to Git** - Use GitHub secrets or external secret managers
2. ✅ **Use placeholder values for vendoring** - Already implemented via `values.public.yaml`
3. ✅ **Rotate secrets periodically** - Change `MYSQL_ROOT_PASSWORD` every 90 days
4. ✅ **Use least-privilege service accounts** - GCP service account should only have Artifact Registry access
5. ✅ **Store production values separately** - Keep `values.prod.yaml` in `.gitignore`

---

## Verification

After setting secrets, verify they're configured:

```bash
# Check if secrets are set (will show names only, not values)
gh secret list

# Expected output:
# MYSQL_ROOT_PASSWORD  Updated YYYY-MM-DD
```

If you don't have `gh` CLI:
```
Go to: https://github.com/<your-username>/k8s-compliment-shop/settings/secrets/actions
Verify you see: MYSQL_ROOT_PASSWORD
```
