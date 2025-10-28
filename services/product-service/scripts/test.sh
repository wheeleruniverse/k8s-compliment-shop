#!/bin/bash
# Run tests for product-service
# Usage: ./scripts/test.sh [--watch] [--filter TestName]

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

cd "$PROJECT_ROOT"

ARGS=()

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --watch)
            echo "🔍 Running tests in watch mode..."
            dotnet watch test --project tests/ProductService.Tests/ProductService.Tests.csproj
            exit 0
            ;;
        --filter)
            ARGS+=(--filter "$2")
            shift 2
            ;;
        --coverage)
            ARGS+=(--collect:"XPlat Code Coverage")
            shift
            ;;
        *)
            shift
            ;;
    esac
done

echo "🧪 Running tests..."
dotnet test tests/ProductService.Tests/ProductService.Tests.csproj \
    --configuration Release \
    --verbosity normal \
    "${ARGS[@]}"

echo "✅ Tests complete!"
