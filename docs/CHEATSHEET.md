# .NET CLI Cheat Sheet (for Java Developers)

Quick reference for common .NET CLI commands with Java/Maven equivalents.

## Project Management

| Task | Maven | .NET CLI |
|------|-------|----------|
| Create new project | `mvn archetype:generate` | `dotnet new webapi -n MyService` |
| Add to solution | Edit parent pom.xml | `dotnet sln add MyService/MyService.csproj` |
| Create solution | Create parent pom.xml | `dotnet new sln -n MySolution` |
| List projects | N/A | `dotnet sln list` |

### Examples

```bash
# Create a new web API project
dotnet new webapi -n ProductService

# Create a test project
dotnet new xunit -n ProductService.Tests

# Create a solution file
dotnet new sln -n ProductService

# Add projects to solution
dotnet sln add src/ProductService/ProductService.csproj
dotnet sln add tests/ProductService.Tests/ProductService.Tests.csproj
```

## Dependency Management

| Task | Maven | .NET CLI |
|------|-------|----------|
| Add dependency | Edit pom.xml | `dotnet add package PackageName` |
| Remove dependency | Edit pom.xml | `dotnet remove package PackageName` |
| List dependencies | `mvn dependency:tree` | `dotnet list package` |
| Restore dependencies | `mvn install` (downloads) | `dotnet restore` |
| Update dependencies | `mvn versions:update` | `dotnet list package --outdated` |

### Examples

```bash
# Add a NuGet package
dotnet add package Newtonsoft.Json

# Add specific version
dotnet add package xunit --version 2.6.2

# Remove a package
dotnet remove package Newtonsoft.Json

# List all packages
dotnet list package

# Show outdated packages
dotnet list package --outdated

# Restore all dependencies (rarely needed - build does this)
dotnet restore
```

## Build & Clean

| Task | Maven | .NET CLI |
|------|-------|----------|
| Clean | `mvn clean` | `dotnet clean` |
| Build | `mvn compile` | `dotnet build` |
| Rebuild | `mvn clean compile` | `dotnet clean && dotnet build` |
| Build Release | `mvn package -DskipTests` | `dotnet build -c Release` |

### Examples

```bash
# Clean build outputs
dotnet clean

# Build in Debug mode (default)
dotnet build

# Build in Release mode
dotnet build --configuration Release
dotnet build -c Release  # Short form

# Build without restoring (faster)
dotnet build --no-restore

# Build specific project
dotnet build src/ProductService/ProductService.csproj
```

## Testing

| Task | Maven | .NET CLI |
|------|-------|----------|
| Run tests | `mvn test` | `dotnet test` |
| Run specific test | `mvn test -Dtest=TestClass` | `dotnet test --filter "TestName"` |
| Skip tests | `mvn install -DskipTests` | `dotnet build` (tests separate) |
| Test coverage | `mvn jacoco:report` | `dotnet test --collect:"XPlat Code Coverage"` |
| Watch tests | N/A | `dotnet watch test` |

### Examples

```bash
# Run all tests
dotnet test

# Run tests in specific project
dotnet test tests/ProductService.Tests/ProductService.Tests.csproj

# Run tests matching a filter
dotnet test --filter "ProductTests"
dotnet test --filter "FullyQualifiedName~ProductRepository"

# Run with detailed output
dotnet test --verbosity normal  # or: quiet, minimal, normal, detailed, diagnostic

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Watch mode (re-run on file changes)
dotnet watch test

# Run tests without building (faster)
dotnet test --no-build
```

## Running Applications

| Task | Maven | .NET CLI |
|------|-------|----------|
| Run app | `mvn spring-boot:run` | `dotnet run` |
| Run with profile | `mvn spring-boot:run -Dspring.profiles.active=dev` | `dotnet run --environment Development` |
| Watch mode | N/A | `dotnet watch run` |

### Examples

```bash
# Run the application (builds first)
dotnet run

# Run without building
dotnet run --no-build

# Run with specific environment
dotnet run --environment Production
export ASPNETCORE_ENVIRONMENT=Development && dotnet run

# Watch mode (auto-restart on changes)
dotnet watch run

# Run from specific project
dotnet run --project src/ProductService/ProductService.csproj
```

## Publishing/Packaging

| Task | Maven | .NET CLI |
|------|-------|----------|
| Package | `mvn package` | `dotnet publish` |
| Create JAR | `mvn package` | `dotnet publish -o ./dist` |
| Skip tests | `mvn package -DskipTests` | `dotnet publish` (tests separate) |

### Examples

```bash
# Publish for deployment
dotnet publish

# Publish to specific directory
dotnet publish -o ./publish

# Publish Release build
dotnet publish -c Release

# Publish self-contained (includes .NET runtime)
dotnet publish -c Release --self-contained true -r linux-x64

# Publish without building
dotnet publish --no-build
```

## Project Info

| Task | Maven | .NET CLI |
|------|-------|----------|
| List dependencies | `mvn dependency:tree` | `dotnet list package` |
| Show info | `mvn help:effective-pom` | `dotnet msbuild -pp` |

### Examples

```bash
# List all NuGet packages
dotnet list package

# Show transitive dependencies
dotnet list package --include-transitive

# Show project references
dotnet list reference

# Show SDK version
dotnet --version

# List installed SDKs
dotnet --list-sdks

# List installed runtimes
dotnet --list-runtimes
```

## Formatting & Linting

| Task | Maven | .NET CLI |
|------|-------|----------|
| Format code | `mvn fmt:format` | `dotnet format` |
| Check format | `mvn fmt:check` | `dotnet format --verify-no-changes` |

### Examples

```bash
# Format all files
dotnet format

# Check formatting without changing
dotnet format --verify-no-changes

# Format specific project
dotnet format src/ProductService/ProductService.csproj
```

## Entity Framework (Like JPA/Hibernate)

| Task | Maven/JPA | .NET CLI |
|------|-----------|----------|
| Generate migration | Flyway/Liquibase | `dotnet ef migrations add MigrationName` |
| Apply migrations | `mvn flyway:migrate` | `dotnet ef database update` |
| Revert migration | `mvn flyway:undo` | `dotnet ef database update PreviousMigration` |
| Drop database | SQL script | `dotnet ef database drop` |

### Examples

```bash
# Install EF Core tools (one-time)
dotnet tool install --global dotnet-ef

# Create a migration
dotnet ef migrations add InitialCreate

# Apply migrations to database
dotnet ef database update

# Revert to specific migration
dotnet ef database update PreviousMigration

# Remove last migration (if not applied)
dotnet ef migrations remove

# Generate SQL script
dotnet ef migrations script

# Drop database
dotnet ef database drop
```

## Docker Integration

### Build with Tests

```bash
# Build and run tests in Docker
docker build --target test -t product-service:test .

# Build final image (skip tests)
docker build --target final -t product-service:latest .
```

## Helper Scripts (in ./scripts/)

```bash
# Build project
./scripts/build.sh
./scripts/build.sh --with-tests

# Run tests
./scripts/test.sh
./scripts/test.sh --watch
./scripts/test.sh --filter ProductTests
./scripts/test.sh --coverage

# Build Docker image
./scripts/docker-build.sh
./scripts/docker-build.sh --with-tests
./scripts/docker-build.sh --tag myimage:v1.0

# Run locally
./scripts/run-local.sh
```

## Common Workflows

### Full Build + Test Cycle

```bash
# Java
mvn clean install

# .NET (tests separate from build)
dotnet clean
dotnet build
dotnet test
```

### Quick Development Iteration

```bash
# Java
mvn spring-boot:run

# .NET (with auto-reload)
dotnet watch run
```

### Prepare for Deployment

```bash
# Java
mvn clean package -DskipTests

# .NET
dotnet clean
dotnet publish -c Release
```

### Add New Dependency

```bash
# Java
# 1. Edit pom.xml
# 2. mvn install

# .NET
dotnet add package PackageName
dotnet restore  # (optional - build will do this)
```

## Tips for Java Developers

1. **No need to manually restore** - `dotnet build` and `dotnet run` auto-restore
2. **Tests are separate** - Unlike `mvn test`, you explicitly run `dotnet test`
3. **Watch mode is built-in** - `dotnet watch run` / `dotnet watch test`
4. **No Maven archetypes** - Use `dotnet new [template]` for project templates
5. **Global tools** - Install CLI tools globally: `dotnet tool install -g <tool>`

## Project Templates

```bash
# List available templates
dotnet new list

# Common templates
dotnet new console      # Console application
dotnet new classlib     # Class library
dotnet new webapi       # ASP.NET Core Web API
dotnet new mvc          # ASP.NET Core MVC
dotnet new grpc         # gRPC service
dotnet new xunit        # xUnit test project
dotnet new nunit        # NUnit test project
dotnet new mstest       # MSTest test project
```

## Quick Reference: Build → Test → Run

```bash
# Development workflow
dotnet build              # Compile
dotnet test               # Run tests
dotnet run                # Start app

# Watch mode (auto-reload)
dotnet watch test         # Tests
dotnet watch run          # App

# Production build
dotnet publish -c Release # Package for deployment
```

## More Help

```bash
# Get help on any command
dotnet --help
dotnet build --help
dotnet test --help

# .NET Documentation
# https://docs.microsoft.com/en-us/dotnet/core/tools/
```
