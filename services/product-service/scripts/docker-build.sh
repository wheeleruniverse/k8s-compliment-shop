#!/bin/bash
# Build Docker image for product-service
# Usage: ./scripts/docker-build.sh [--with-tests] [--tag custom-tag]

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

TAG="product-service:latest"
TARGET="final"
WITH_TESTS=false

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --with-tests)
            WITH_TESTS=true
            TARGET="test"
            shift
            ;;
        --tag)
            TAG="$2"
            shift 2
            ;;
        *)
            shift
            ;;
    esac
done

cd "$PROJECT_ROOT"

if [ "$WITH_TESTS" = true ]; then
    echo "🧪 Building Docker image WITH tests..."
    echo "   (Tests will run during build and must pass)"
    docker build \
        --target test \
        -t "${TAG}-test" \
        .

    # Now build final image
    echo "🐳 Building final Docker image..."
    docker build \
        --target final \
        -t "$TAG" \
        .
else
    echo "🐳 Building Docker image (skipping tests)..."
    docker build \
        --target final \
        -t "$TAG" \
        .
fi

echo "✅ Docker image built: $TAG"

# Show image info
docker images | grep product-service | head -1
