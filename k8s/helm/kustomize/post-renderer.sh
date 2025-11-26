#!/bin/bash
# Kustomize post-renderer script for Helm
# This script is called by Helm with the rendered templates on stdin

set -e

# Get the directory where this script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Save stdin to the stdin.yaml file
cat > "$SCRIPT_DIR/stdin.yaml"

# Run kustomize build on this directory
kustomize build "$SCRIPT_DIR"

# Clean up
rm -f "$SCRIPT_DIR/stdin.yaml"
