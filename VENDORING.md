# Kubernetes Manifest Vendoring Guide

This project uses a vendoring approach to render and commit Kubernetes manifests for better GitOps practices and PR visibility.

## Overview

**Vendoring** means pre-rendering your Helm + Kustomize templates into final YAML manifests and committing them to Git. This gives you:

- ✅ Full visibility of what will be deployed in every PR
- ✅ Audit trail of all infrastructure changes
- ✅ Safe for public repositories (uses placeholder secrets)
- ✅ No Helm required in the cluster (ArgoCD can apply directly)

## Security Model

### Public Repository Safety

This repository is public, so we use a **placeholder-based approach** for secrets:

1. **Development/Vendoring**: Uses `values.public.yaml` with placeholder values
   - Safe to commit to public repository
   - Used by `./scripts/vendor-manifests.sh`
   - Example: `mysql.auth.rootPassword: "PLACEHOLDER_REPLACE_AT_DEPLOY_TIME"`

2. **Production Deployment**: Real secrets provided at deploy time
   - Via Helm `--set` flags
   - Via private `values.prod.yaml` (NOT committed to Git, in `.gitignore`)
   - Via CI/CD secrets (GitHub Actions secrets, etc.)

### Files

```
k8s/helm/
├── values.yaml              # Base values (contains placeholder password)
├── values.public.yaml       # Public-safe values for vendoring (DO COMMIT)
└── values.prod.yaml         # Real secrets (DO NOT COMMIT - in .gitignore)
```

## Usage

### Generate Vendored Manifests

```bash
./scripts/vendor-manifests.sh
```

This generates manifests in `k8s/rendered/` using placeholder values.

### Review Changes

```bash
git diff k8s/rendered/
```

Review the exact changes that will be deployed.

### Commit for PR Review

```bash
git add k8s/rendered/
git commit -m "Update manifests for feature XYZ"
```

Reviewers can see exactly what Kubernetes resources change.

### Deploy to Production

#### Option 1: Helm with Secret Override

```bash
helm upgrade --install k8s-compliment-shop k8s/helm \
  --set mysql.auth.rootPassword="$MYSQL_ROOT_PASSWORD" \
  --namespace default
```

#### Option 2: Helm with Private Values File

```bash
# Create values.prod.yaml locally (not in Git)
cat > k8s/helm/values.prod.yaml <<EOF
mysql:
  auth:
    rootPassword: "your-real-secure-password"
EOF

# Deploy
helm upgrade --install k8s-compliment-shop k8s/helm \
  --values k8s/helm/values.public.yaml \
  --values k8s/helm/values.prod.yaml \
  --namespace default
```

#### Option 3: ArgoCD with External Secrets

Configure ArgoCD to apply the vendored manifests and use External Secrets Operator or Sealed Secrets to inject real secrets at runtime.

## Workflow

1. **Develop**: Make changes to Helm templates
2. **Vendor**: Run `./scripts/vendor-manifests.sh`
3. **Review**: Check `git diff k8s/rendered/`
4. **Commit**: Commit the rendered manifests
5. **PR**: Create PR - reviewers see exact K8s changes
6. **Deploy**: ArgoCD/Helm applies with real secrets

## What Gets Vendored?

- ✅ Deployments
- ✅ Services
- ✅ StatefulSets
- ✅ ConfigMaps
- ✅ Secrets (with PLACEHOLDER values)
- ✅ All Kustomize transformations (labels, annotations, patches)

## What Doesn't Get Committed?

- ❌ `values.prod.yaml` - Contains real secrets
- ❌ Any files with actual credentials
- ❌ Private configuration

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Vendor Manifests
on: [pull_request]

jobs:
  vendor:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Install tools
        run: |
          curl -LO https://get.helm.sh/helm-v3.12.0-linux-amd64.tar.gz
          tar xzf helm-v3.12.0-linux-amd64.tar.gz
          sudo mv linux-amd64/helm /usr/local/bin/

          curl -LO https://github.com/kubernetes-sigs/kustomize/releases/download/kustomize%2Fv5.0.0/kustomize_v5.0.0_linux_amd64.tar.gz
          tar xzf kustomize_v5.0.0_linux_amd64.tar.gz
          sudo mv kustomize /usr/local/bin/

      - name: Generate manifests
        run: ./scripts/vendor-manifests.sh

      - name: Check for changes
        run: |
          if ! git diff --quiet k8s/rendered/; then
            echo "❌ Rendered manifests are out of sync!"
            echo "Run ./scripts/vendor-manifests.sh and commit the changes"
            git diff k8s/rendered/
            exit 1
          fi
```

## Troubleshooting

### "Plugin not found" error

This was fixed by not using Helm's `--post-renderer` flag and instead manually piping through Kustomize.

### Hardcoded passwords in rendered manifests

All passwords should show `PLACEHOLDER_REPLACE_AT_DEPLOY_TIME`. If you see real passwords, check:
- You're using `values.public.yaml`
- The vendoring script references the correct values file
- Probes and init containers use environment variables, not hardcoded values

### Kustomize deprecation warnings

Fixed by updating to newer syntax:
- `commonLabels` → `labels`
- `patchesStrategicMerge` → `patches`

## Best Practices

1. **Always vendor before committing** - Run the script after any Helm changes
2. **Review rendered diffs** - Check what actually changes in Kubernetes
3. **Never commit real secrets** - Use placeholders or external secret management
4. **Keep values.prod.yaml private** - Add to `.gitignore`
5. **Use CI to enforce** - Add checks to ensure manifests are up-to-date
