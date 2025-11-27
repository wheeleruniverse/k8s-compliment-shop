# Kustomize Post-Renderer

This directory contains Kustomize configurations used as a post-renderer for the Helm chart.

## How It Works

1. **Helm renders templates** using values from `values.yaml`
2. **Kustomize applies patches** to the rendered manifests
3. **Final output** is deployed by ArgoCD or kubectl

## Using Kustomize as a Post-Renderer

### Local Testing

```bash
# Render with Helm + Kustomize post-renderer
helm template k8s-compliment-shop ../helm \
  --post-renderer ../helm/kustomize/post-renderer.sh \
  --namespace default

# Or use the vendoring script
cd ../../
./scripts/vendor-manifests.sh
```

### ArgoCD Configuration

ArgoCD supports Kustomize post-rendering natively. See `../argocd/application.yaml` for configuration.

## Directory Structure

```
kustomize/
├── kustomization.yaml           # Main kustomization config
├── stdin.yaml                   # Placeholder for Helm output
├── patches/                     # Patch files
│   ├── *.yaml.example          # Example patches (rename to .yaml to use)
└── README.md                    # This file
```

## Example Patches

### Monitoring Labels

Uncomment in `kustomization.yaml`:
```yaml
patchesStrategicMerge:
  - patches/add-monitoring-labels.yaml
```

### Security Contexts

Uncomment in `kustomization.yaml`:
```yaml
patchesStrategicMerge:
  - patches/security-context.yaml
```

## Common Use Cases

1. **Add labels/annotations** - Use `commonLabels` or `commonAnnotations`
2. **Override images** - Use `images` section
3. **Patch resources** - Add strategic merge patches or JSON patches
4. **Environment-specific changes** - Create separate kustomization files

## Tips

- Keep patches minimal and focused
- Use strategic merge for simple changes
- Use JSON patches for complex transformations
- Test locally before committing
