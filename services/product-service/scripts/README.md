# Helper Scripts

Convenience scripts for common development tasks.

## Available Scripts

### 🔨 build.sh
Build the product-service project.

```bash
# Standard build
./scripts/build.sh

# Build and run tests
./scripts/build.sh --with-tests
```

**What it does:**
1. Cleans previous builds
2. Restores NuGet dependencies
3. Builds in Release configuration
4. Optionally runs tests

**Equivalent commands:**
```bash
dotnet clean
dotnet restore
dotnet build --configuration Release
```

---

### 🧪 test.sh
Run unit tests.

```bash
# Run all tests
./scripts/test.sh

# Run tests in watch mode (auto-rerun on changes)
./scripts/test.sh --watch

# Run specific tests
./scripts/test.sh --filter ProductTests

# Run with code coverage
./scripts/test.sh --coverage
```

**What it does:**
- Runs xUnit tests with configurable options
- Watch mode for TDD workflow
- Filter by test name
- Generate code coverage reports

**Equivalent commands:**
```bash
dotnet test --verbosity normal
dotnet watch test
dotnet test --filter "ProductTests"
dotnet test --collect:"XPlat Code Coverage"
```

---

### 🐳 docker-build.sh
Build Docker image for product-service.

```bash
# Build without running tests
./scripts/docker-build.sh

# Build WITH tests (tests must pass for build to succeed)
./scripts/docker-build.sh --with-tests

# Custom tag
./scripts/docker-build.sh --tag myregistry/product-service:v1.0
```

**What it does:**
- Builds Docker image using multi-stage Dockerfile
- Optionally runs tests during build
- Tags image with custom or default tag

**Equivalent commands:**
```bash
# Without tests
docker build --target final -t product-service:latest .

# With tests
docker build --target test -t product-service:test .
docker build --target final -t product-service:latest .
```

---

### 🚀 run-local.sh
Run product-service locally with dotnet CLI.

```bash
./scripts/run-local.sh
```

**What it does:**
1. Sets `ASPNETCORE_ENVIRONMENT=Development`
2. Runs `dotnet run` from src/ProductService directory
3. Displays endpoint information

**Prerequisites:**
- MySQL running on localhost:3306
- Database `complimentshop` exists

**Equivalent command:**
```bash
cd src/ProductService
export ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

---

## Quick Start

```bash
# 1. Build the project
./scripts/build.sh

# 2. Run tests
./scripts/test.sh

# 3. Run locally (requires MySQL)
./scripts/run-local.sh

# Or build Docker image
./scripts/docker-build.sh
```

## Testing Workflow

```bash
# Terminal 1: Watch mode (auto-run tests on save)
./scripts/test.sh --watch

# Terminal 2: Make code changes
# Tests automatically re-run when you save files!
```

## Docker Workflow

```bash
# Development: Build and test
./scripts/docker-build.sh --with-tests

# Production: Build only (faster)
./scripts/docker-build.sh --tag product-service:production
```

## Tips

1. **Make scripts executable:**
   ```bash
   chmod +x scripts/*.sh
   ```

2. **Run from project root:**
   ```bash
   # Good
   ./scripts/build.sh

   # Also works from anywhere
   /path/to/project/scripts/build.sh
   ```

3. **Combine with other commands:**
   ```bash
   # Build, test, then run locally
   ./scripts/build.sh --with-tests && ./scripts/run-local.sh

   # Build Docker image after tests pass
   ./scripts/test.sh && ./scripts/docker-build.sh
   ```

## Java Developer Notes

If you're coming from Maven, these scripts are roughly equivalent to:

| Script | Maven Equivalent |
|--------|------------------|
| `build.sh` | `mvn clean compile` |
| `build.sh --with-tests` | `mvn clean install` |
| `test.sh` | `mvn test` |
| `docker-build.sh` | `mvn package docker:build` |
| `run-local.sh` | `mvn spring-boot:run` |

## Troubleshooting

### Permission Denied

```bash
chmod +x scripts/*.sh
```

### Script Not Found

Make sure you're in the project root:
```bash
cd /path/to/product-service
./scripts/build.sh
```

### MySQL Connection Error

Ensure MySQL is running:
```bash
docker ps | grep mysql

# If not running:
docker start mysql
```

## Adding Your Own Scripts

1. Create script in `scripts/` directory
2. Add shebang: `#!/bin/bash`
3. Make executable: `chmod +x scripts/yourscript.sh`
4. Use absolute paths based on script location:
   ```bash
   SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
   PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
   ```
