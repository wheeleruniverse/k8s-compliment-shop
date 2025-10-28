#!/bin/bash
# Build the product-service
# Usage: ./scripts/build.sh [--with-tests]

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "🔨 Building product-service..."

cd "$PROJECT_ROOT"

# Clean previous builds
echo "📦 Cleaning previous builds..."
dotnet clean

# Restore dependencies
echo "📥 Restoring dependencies..."
dotnet restore

# Build
echo "🏗️  Building..."
dotnet build --configuration Release --no-restore

# Run tests if requested
if [[ "$1" == "--with-tests" ]]; then
    echo "🧪 Running tests..."
    dotnet test --configuration Release --no-build --verbosity normal
fi

echo "✅ Build complete!"
