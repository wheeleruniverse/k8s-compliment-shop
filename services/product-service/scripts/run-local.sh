#!/bin/bash
# Run product-service locally with dotnet run
# Usage: ./scripts/run-local.sh

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "🚀 Starting product-service locally..."
echo ""
echo "Prerequisites:"
echo "  - MySQL running on localhost:3306"
echo "  - Database 'complimentshop' exists"
echo ""
echo "Endpoints:"
echo "  - HTTP (Health): http://localhost:8080/health"
echo "  - gRPC:         localhost:8081"
echo ""

cd "$PROJECT_ROOT/src/ProductService"

# Set environment for local development
export ASPNETCORE_ENVIRONMENT=Development

dotnet run
