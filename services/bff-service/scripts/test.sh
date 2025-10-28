#!/bin/bash

# Test script for BFF Service
# Usage: ./scripts/test.sh [--watch] [--coverage]

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Parse arguments
WATCH_MODE=false
COVERAGE=false

for arg in "$@"; do
    case $arg in
        --watch)
            WATCH_MODE=true
            ;;
        --coverage)
            COVERAGE=true
            ;;
    esac
done

echo -e "${GREEN}🧪 Running BFF Service Tests...${NC}"

if [ "$WATCH_MODE" = true ]; then
    echo -e "${YELLOW}👀 Running in watch mode (Ctrl+C to exit)...${NC}"
    dotnet watch test tests/BffService.Tests/BffService.Tests.csproj --verbosity normal
elif [ "$COVERAGE" = true ]; then
    echo -e "${YELLOW}📊 Running with coverage...${NC}"
    dotnet test tests/BffService.Tests/BffService.Tests.csproj \
        --configuration Release \
        --collect:"XPlat Code Coverage" \
        --verbosity normal
    echo -e "${GREEN}Coverage report generated in tests/BffService.Tests/TestResults/${NC}"
else
    dotnet test tests/BffService.Tests/BffService.Tests.csproj \
        --configuration Release \
        --verbosity normal
fi

echo -e "${GREEN}✅ Tests complete!${NC}"
