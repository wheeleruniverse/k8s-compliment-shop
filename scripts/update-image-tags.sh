#!/bin/bash
# Script to update Docker image tags in Helm values.yaml
# Usage: ./scripts/update-image-tags.sh <service-name> <new-tag>
# Example: ./scripts/update-image-tags.sh web-service abc123f

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

SERVICE=$1
NEW_TAG=$2
VALUES_FILE="k8s/helm/values.yaml"

# Validation
if [ -z "$SERVICE" ] || [ -z "$NEW_TAG" ]; then
    echo -e "${RED}Error: Missing required arguments${NC}"
    echo "Usage: $0 <service-name> <new-tag>"
    echo "Services: web-service, bff-service, product-service, all"
    echo "Example: $0 web-service abc123f"
    exit 1
fi

if [ ! -f "$VALUES_FILE" ]; then
    echo -e "${RED}Error: $VALUES_FILE not found${NC}"
    exit 1
fi

# Function to update a specific service tag
update_service_tag() {
    local service=$1
    local tag=$2
    local section=""

    case $service in
        web-service)
            section="webService"
            ;;
        bff-service)
            section="bffService"
            ;;
        product-service)
            section="productService"
            ;;
        *)
            echo -e "${RED}Error: Unknown service $service${NC}"
            echo "Valid services: web-service, bff-service, product-service"
            exit 1
            ;;
    esac

    echo -e "${YELLOW}Updating $service tag to $tag...${NC}"

    # Use a more robust approach with awk instead of sed for cross-platform compatibility
    awk -v section="$section:" -v tag="$tag" '
        BEGIN { in_section = 0 }
        $0 ~ section { in_section = 1 }
        in_section && /^[a-zA-Z]/ && $0 !~ section { in_section = 0 }
        in_section && /^  tag:/ {
            sub(/tag:.*/, "tag: " tag)
        }
        { print }
    ' "$VALUES_FILE" > "${VALUES_FILE}.tmp" && mv "${VALUES_FILE}.tmp" "$VALUES_FILE"

    echo -e "${GREEN}✓ Updated $service to tag $tag${NC}"
}

# Update service(s)
if [ "$SERVICE" == "all" ]; then
    update_service_tag "web-service" "$NEW_TAG"
    update_service_tag "bff-service" "$NEW_TAG"
    update_service_tag "product-service" "$NEW_TAG"
else
    update_service_tag "$SERVICE" "$NEW_TAG"
fi

echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Image tags updated successfully!${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo "Updated services in $VALUES_FILE"
echo ""
echo "Next steps:"
echo "  1. Review the changes: git diff $VALUES_FILE"
echo "  2. Vendor the manifests: ./scripts/vendor-manifests.sh"
echo "  3. Commit the changes: git add k8s/ && git commit -m 'Update image tags to $NEW_TAG'"
echo ""
