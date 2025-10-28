#!/bin/bash

# Build script for BFF Service
# Usage: ./scripts/build.sh [--with-tests]

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Parse arguments
RUN_TESTS=false
if [[ "$1" == "--with-tests" ]]; then
    RUN_TESTS=true
fi

echo -e "${GREEN}🔨 Building BFF Service...${NC}"

# Restore dependencies
echo -e "${YELLOW}📦 Restoring dependencies...${NC}"
dotnet restore src/BffService/BffService.csproj

# Build the project
echo -e "${YELLOW}🏗️  Building project...${NC}"
dotnet build src/BffService/BffService.csproj --configuration Release --no-restore

if [ "$RUN_TESTS" = true ]; then
    echo -e "${YELLOW}🧪 Running tests...${NC}"
    dotnet test tests/BffService.Tests/BffService.Tests.csproj \
        --configuration Release \
        --no-restore \
        --verbosity normal
fi

echo -e "${GREEN}✅ Build complete!${NC}"
