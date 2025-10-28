#!/bin/bash

# Run BFF Service locally
# Usage: ./scripts/run-local.sh

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check if product-service is running
echo -e "${YELLOW}🔍 Checking if product-service is available...${NC}"
if curl -s -f http://localhost:8080/health > /dev/null 2>&1; then
    echo -e "${GREEN}✅ product-service is running${NC}"
else
    echo -e "${RED}❌ product-service is not running on http://localhost:8080${NC}"
    echo -e "${YELLOW}💡 Start product-service first:${NC}"
    echo -e "   cd ../product-service && ./scripts/run-local.sh"
    exit 1
fi

echo -e "${GREEN}🚀 Starting BFF Service...${NC}"
echo -e "${YELLOW}📍 GraphQL endpoint: http://localhost:8082/graphql${NC}"
echo -e "${YELLOW}📍 Health endpoint: http://localhost:8082/health${NC}"
echo -e "${YELLOW}📍 Product Service URL: http://localhost:8081${NC}"
echo ""
echo -e "${YELLOW}Press Ctrl+C to stop${NC}"
echo ""

# Set environment for local development
export ASPNETCORE_ENVIRONMENT=Development
export ProductService__Url=http://localhost:8081

# Run the service
dotnet run --project src/BffService/BffService.csproj
