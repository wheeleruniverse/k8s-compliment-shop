#!/bin/bash

# Docker build script for BFF Service
# Usage: ./scripts/docker-build.sh [--with-tests] [--no-cache]

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Default values
IMAGE_NAME="bff-service"
TAG="latest"
RUN_TESTS=false
NO_CACHE=""

# Parse arguments
for arg in "$@"; do
    case $arg in
        --with-tests)
            RUN_TESTS=true
            ;;
        --no-cache)
            NO_CACHE="--no-cache"
            ;;
    esac
done

if [ "$RUN_TESTS" = true ]; then
    echo -e "${GREEN}🧪 Building Docker image WITH tests...${NC}"
    echo -e "${YELLOW}   (Tests will run during build and must pass)${NC}"

    # First, build to test stage to run tests
    echo -e "${YELLOW}   Step 1/2: Running tests...${NC}"
    docker build $NO_CACHE --target test -t ${IMAGE_NAME}:test .

    # If tests pass, build the full image
    echo -e "${YELLOW}   Step 2/2: Building final image...${NC}"
    docker build -t ${IMAGE_NAME}:${TAG} .
else
    echo -e "${GREEN}🐳 Building Docker image WITHOUT tests...${NC}"
    echo -e "${YELLOW}   (Skipping test stage for faster builds)${NC}"
    docker build $NO_CACHE -t ${IMAGE_NAME}:${TAG} .
fi

echo -e "${GREEN}✅ Docker image built successfully: ${IMAGE_NAME}:${TAG}${NC}"
echo -e "${YELLOW}💡 To run: docker run -p 8082:8082 ${IMAGE_NAME}:${TAG}${NC}"
