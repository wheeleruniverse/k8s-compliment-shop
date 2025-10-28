# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY src/BffService/BffService.csproj src/BffService/
RUN dotnet restore src/BffService/BffService.csproj

# Copy source code
COPY src/BffService/ src/BffService/

# Build
WORKDIR /src/src/BffService
RUN dotnet build BffService.csproj -c Release -o /app/build

# Test stage (optional - skip with: docker build --target publish)
FROM build AS test
WORKDIR /src

# Copy test project
COPY tests/BffService.Tests/BffService.Tests.csproj tests/BffService.Tests/
RUN dotnet restore tests/BffService.Tests/BffService.Tests.csproj

COPY tests/BffService.Tests/ tests/BffService.Tests/

# Run tests
RUN dotnet test tests/BffService.Tests/BffService.Tests.csproj \
    --configuration Release \
    --no-restore \
    --verbosity normal \
    --logger "trx;LogFileName=/test-results/test-results.trx"

# Publish stage
FROM build AS publish
RUN dotnet publish BffService.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published app
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8082

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8082
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8082/health || exit 1

# Run the app
ENTRYPOINT ["dotnet", "BffService.dll"]
