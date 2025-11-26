#!/bin/bash
# Script to vendor (render and save) Kubernetes manifests from Helm + Kustomize
# This allows you to review the exact manifests that will be applied to the cluster

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
HELM_CHART_DIR="k8s/helm"
OUTPUT_DIR="k8s/rendered"
RELEASE_NAME="k8s-compliment-shop"
NAMESPACE="default"

echo -e "${YELLOW}Vendoring Kubernetes manifests...${NC}"

# Check if required tools are installed
command -v helm >/dev/null 2>&1 || { echo -e "${RED}Error: helm is not installed${NC}" >&2; exit 1; }
command -v kustomize >/dev/null 2>&1 || { echo -e "${RED}Error: kustomize is not installed${NC}" >&2; exit 1; }

# Create output directory
mkdir -p "$OUTPUT_DIR"

# Clean previous rendered manifests
echo -e "${YELLOW}Cleaning previous rendered manifests...${NC}"
rm -rf "$OUTPUT_DIR"/*

# Render Helm templates with Kustomize post-renderer
echo -e "${YELLOW}Rendering Helm templates with Kustomize post-renderer...${NC}"
helm template "$RELEASE_NAME" "$HELM_CHART_DIR" \
  --namespace "$NAMESPACE" \
  --post-renderer "$HELM_CHART_DIR/kustomize/post-renderer.sh" \
  > "$OUTPUT_DIR/all-resources.yaml"

echo -e "${GREEN}✓ Manifests rendered successfully!${NC}"
echo -e "${GREEN}✓ Output: $OUTPUT_DIR/all-resources.yaml${NC}"

# Optionally split into separate files by resource type
echo -e "${YELLOW}Splitting manifests by resource type...${NC}"

# Use yq or csplit to split the file (if yq is available)
if command -v yq >/dev/null 2>&1; then
  # Create subdirectories
  mkdir -p "$OUTPUT_DIR"/{deployments,services,statefulsets,configmaps,secrets}

  # Extract each resource type (requires yq v4+)
  yq eval 'select(.kind == "Deployment")' "$OUTPUT_DIR/all-resources.yaml" > "$OUTPUT_DIR/deployments/deployments.yaml" 2>/dev/null || true
  yq eval 'select(.kind == "Service")' "$OUTPUT_DIR/all-resources.yaml" > "$OUTPUT_DIR/services/services.yaml" 2>/dev/null || true
  yq eval 'select(.kind == "StatefulSet")' "$OUTPUT_DIR/all-resources.yaml" > "$OUTPUT_DIR/statefulsets/statefulsets.yaml" 2>/dev/null || true
  yq eval 'select(.kind == "ConfigMap")' "$OUTPUT_DIR/all-resources.yaml" > "$OUTPUT_DIR/configmaps/configmaps.yaml" 2>/dev/null || true
  yq eval 'select(.kind == "Secret")' "$OUTPUT_DIR/all-resources.yaml" > "$OUTPUT_DIR/secrets/secrets.yaml" 2>/dev/null || true

  # Remove empty files
  find "$OUTPUT_DIR" -type f -size 0 -delete

  echo -e "${GREEN}✓ Manifests split by type${NC}"
else
  echo -e "${YELLOW}Note: yq not found. Skipping split by resource type.${NC}"
  echo -e "${YELLOW}Install yq to split manifests: https://github.com/mikefarah/yq${NC}"
fi

# Show summary
echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Vendoring Complete!${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo -e "Rendered manifests are in: ${YELLOW}$OUTPUT_DIR${NC}"
echo ""
echo -e "Next steps:"
echo -e "  1. Review the rendered manifests"
echo -e "  2. Commit to Git for review in PRs"
echo -e "  3. ArgoCD will apply these when synced"
echo ""
