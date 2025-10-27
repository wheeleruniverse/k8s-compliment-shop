# Local Testing Guide

This guide walks you through testing the product-service locally using Docker Desktop before deploying to Kubernetes.

## Prerequisites

- Docker Desktop installed and **running**
- .NET 8 SDK (optional, for building outside Docker)
- grpcurl installed for testing gRPC endpoints

### Install grpcurl

```bash
# macOS (Homebrew)
brew install grpcurl

# Or download from: https://github.com/fullstorydev/grpcurl/releases
```

## Step 1: Start Docker Desktop

1. Open Docker Desktop application
2. Wait for Docker to fully start (whale icon in menu bar should be stable)
3. Verify Docker is running:
   ```bash
   docker --version
   docker ps
   ```

## Step 2: Start MySQL Container

Run MySQL 8.0 locally with the password configured in the product-service:

```bash
# Start MySQL container
docker run -d \
  --name mysql \
  -e MYSQL_ROOT_PASSWORD=yourpassword \
  -e MYSQL_DATABASE=complimentshop \
  -p 3306:3306 \
  mysql:8.0

# Verify MySQL is running
docker ps | grep mysql

# Check MySQL logs (should see "ready for connections")
docker logs mysql
```

### Verify MySQL Connection

```bash
# Connect to MySQL (password: yourpassword)
docker exec -it mysql mysql -uroot -pyourpassword -e "SHOW DATABASES;"

# Should see 'complimentshop' database
```

## Step 3: Build Product Service Image

Build the Docker image for product-service:

```bash
cd services/product-service

# Build the image
docker build -t product-service:latest .

# Verify image was created
docker images | grep product-service
```

### Expected Build Output

You should see:
- Restore packages
- Build project
- Publish application
- Final image created (~200MB)

## Step 4: Run Product Service Container

Start the product-service and connect it to MySQL:

```bash
# Run product-service
docker run -d \
  --name product-service \
  --link mysql:mysql \
  -p 8080:8080 \
  -p 8081:8081 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e ConnectionStrings__DefaultConnection="Server=mysql;Port=3306;Database=complimentshop;User=root;Password=yourpassword;" \
  product-service:latest

# Verify container is running
docker ps | grep product-service

# Check logs (should see migrations applied and "Application started")
docker logs product-service

# Follow logs in real-time
docker logs -f product-service
```

### Troubleshooting

If the container exits immediately:

```bash
# Check exit reason
docker logs product-service

# Common issues:
# - MySQL not ready: Wait 10-15 seconds after starting MySQL
# - Connection string error: Check environment variable format
# - Port conflict: Change -p 8080:8081 to different host port
```

## Step 5: Test Health Endpoint

Verify the service is healthy:

```bash
# Test health check
curl http://localhost:8080/health

# Expected response:
# Healthy

# Test root endpoint
curl http://localhost:8080/

# Expected response:
# "Product Service - gRPC communication only..."
```

## Step 6: Test gRPC Endpoints

Use grpcurl to test the gRPC API:

### List Available Services

```bash
grpcurl -plaintext localhost:8080 list
```

**Expected output:**
```
grpc.health.v1.Health
grpc.reflection.v1alpha.ServerReflection
product.ProductService
```

### List Methods on ProductService

```bash
grpcurl -plaintext localhost:8080 list product.ProductService
```

**Expected output:**
```
product.ProductService.CreateProduct
product.ProductService.DeleteProduct
product.ProductService.GetProduct
product.ProductService.GetProductJsonLd
product.ProductService.ListProducts
product.ProductService.UpdateProduct
```

### Test GetProduct (Get product ID 1)

```bash
grpcurl -plaintext -d '{"id": 1}' localhost:8080 product.ProductService/GetProduct
```

**Expected output:**
```json
{
  "id": 1,
  "name": "Excellent Haircut Compliment",
  "description": "A genuine compliment about your fantastic hairstyle that brightens your day",
  "category": "Appearance",
  "price": 0,
  "currency": "USD",
  "isAvailable": true,
  "createdAt": "2025-10-27T...",
  "updatedAt": "2025-10-27T..."
}
```

### Test ListProducts (All products)

```bash
grpcurl -plaintext -d '{}' localhost:8080 product.ProductService/ListProducts
```

**Expected output:** All 6 seeded products with pagination info

### Test ListProducts (Filter by category)

```bash
# Get only Appearance compliments
grpcurl -plaintext -d '{"category": "Appearance"}' localhost:8080 product.ProductService/ListProducts

# Get only Professional compliments
grpcurl -plaintext -d '{"category": "Professional"}' localhost:8080 product.ProductService/ListProducts

# Get only Personal compliments
grpcurl -plaintext -d '{"category": "Personal"}' localhost:8080 product.ProductService/ListProducts
```

### Test GetProductJsonLd (Get JSON-LD format)

```bash
grpcurl -plaintext -d '{"id": 1}' localhost:8080 product.ProductService/GetProductJsonLd
```

**Expected output:**
```json
{
  "jsonLd": "{\n  \"@context\": \"https://schema.org\",\n  \"@type\": \"Product\",\n  \"productID\": \"1\",\n  \"name\": \"Excellent Haircut Compliment\",\n  \"description\": \"A genuine compliment about your fantastic hairstyle that brightens your day\",\n  \"category\": \"Appearance\",\n  \"offers\": {\n    \"@type\": \"Offer\",\n    \"price\": \"0.00\",\n    \"priceCurrency\": \"USD\",\n    \"availability\": \"https://schema.org/InStock\"\n  }\n}"
}
```

### Test CreateProduct

```bash
grpcurl -plaintext -d '{
  "name": "Awesome Presentation Compliment",
  "description": "Recognition for your engaging and clear presentation skills",
  "category": "Professional",
  "price": 0.0,
  "currency": "USD",
  "is_available": true
}' localhost:8080 product.ProductService/CreateProduct
```

**Expected output:** New product with generated ID (should be 7)

### Test UpdateProduct

```bash
grpcurl -plaintext -d '{
  "id": 7,
  "name": "Outstanding Presentation Compliment",
  "description": "Recognition for your exceptionally engaging and crystal-clear presentation skills",
  "category": "Professional",
  "price": 0.0,
  "currency": "USD",
  "is_available": true
}' localhost:8080 product.ProductService/UpdateProduct
```

### Test DeleteProduct

```bash
grpcurl -plaintext -d '{"id": 7}' localhost:8080 product.ProductService/DeleteProduct
```

**Expected output:**
```json
{
  "success": true,
  "message": "Product 7 deleted successfully"
}
```

## Step 7: Verify Database Contents

Check that data is actually in MySQL:

```bash
# Connect to MySQL
docker exec -it mysql mysql -uroot -pyourpassword complimentshop

# Run queries
SELECT * FROM Products;
SELECT * FROM Products WHERE Category = 'Appearance';
SELECT COUNT(*) FROM Products;

# Exit MySQL
exit
```

## Cleanup

When you're done testing:

```bash
# Stop containers
docker stop product-service mysql

# Remove containers
docker rm product-service mysql

# Remove image (optional)
docker rmi product-service:latest

# Or clean up everything at once
docker stop product-service mysql && docker rm product-service mysql
```

## Restart After Cleanup

To restart testing:

```bash
# 1. Start MySQL (will create new database)
docker run -d --name mysql -e MYSQL_ROOT_PASSWORD=yourpassword -e MYSQL_DATABASE=complimentshop -p 3306:3306 mysql:8.0

# 2. Wait 15 seconds for MySQL to be ready
sleep 15

# 3. Start product-service
docker run -d --name product-service --link mysql:mysql -p 8080:8080 -p 8081:8081 -e ASPNETCORE_ENVIRONMENT=Development -e ConnectionStrings__DefaultConnection="Server=mysql;Port=3306;Database=complimentshop;User=root;Password=yourpassword;" product-service:latest

# 4. Check logs
docker logs -f product-service
```

## Common Issues

### Issue: "Cannot connect to MySQL server"

**Solution:**
- MySQL takes 10-15 seconds to be ready after starting
- Wait and restart product-service:
  ```bash
  docker restart product-service
  docker logs -f product-service
  ```

### Issue: "Port 3306 already allocated"

**Solution:**
- Another MySQL instance is running
- Stop it: `docker stop mysql` or change port mapping: `-p 3307:3306`

### Issue: "Failed to apply migrations"

**Solution:**
- Check MySQL logs: `docker logs mysql`
- Verify connection string in environment variable
- Recreate MySQL container: `docker rm -f mysql` and restart

### Issue: grpcurl connection refused

**Solution:**
- Verify service is running: `docker ps | grep product-service`
- Check logs: `docker logs product-service`
- Verify port mapping: `docker port product-service`

## Next Steps

Once local testing is successful:

1. ✅ product-service works locally
2. 🔲 Create MySQL StatefulSet for Kubernetes
3. 🔲 Deploy product-service to Kubernetes
4. 🔲 Build order-service
5. 🔲 Build bff-service
6. 🔲 Build web-service

## Testing Checklist

- [ ] Docker Desktop is running
- [ ] MySQL container started successfully
- [ ] MySQL is accepting connections
- [ ] Product-service image built successfully
- [ ] Product-service container running
- [ ] Health endpoint returns "Healthy"
- [ ] gRPC reflection working (can list services)
- [ ] GetProduct returns seeded data
- [ ] ListProducts returns all 6 products
- [ ] Category filtering works
- [ ] JSON-LD format is correct
- [ ] CreateProduct adds new product
- [ ] UpdateProduct modifies existing product
- [ ] DeleteProduct removes product
- [ ] Data persists in MySQL database

All checkboxes completed? **Product-service is ready!** 🚀
