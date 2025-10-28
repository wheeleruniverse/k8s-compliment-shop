# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY src/ProductService/ProductService.csproj src/ProductService/
RUN dotnet restore src/ProductService/ProductService.csproj

# Copy everything else and build
COPY src/ProductService/ src/ProductService/
WORKDIR /src/src/ProductService
RUN dotnet build ProductService.csproj -c Release -o /app/build

# Test stage (optional - skip with: --target publish)
FROM build AS test
WORKDIR /src

# Copy test project
COPY tests/ProductService.Tests/ProductService.Tests.csproj tests/ProductService.Tests/
RUN dotnet restore tests/ProductService.Tests/ProductService.Tests.csproj

COPY tests/ProductService.Tests/ tests/ProductService.Tests/

# Run tests
RUN dotnet test tests/ProductService.Tests/ProductService.Tests.csproj \
    --configuration Release \
    --no-restore \
    --verbosity normal \
    --logger "trx;LogFileName=/test-results/test-results.trx"

# Publish stage (depends on build, not test - tests are optional)
FROM build AS publish
RUN dotnet publish ProductService.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProductService.dll"]
