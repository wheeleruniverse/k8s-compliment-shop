# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["WebService.sln", "./"]
COPY ["src/WebService/WebService.csproj", "src/WebService/"]
COPY ["tests/WebService.Tests/WebService.Tests.csproj", "tests/WebService.Tests/"]

# Restore dependencies
RUN dotnet restore "WebService.sln"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/src/WebService"
RUN dotnet build "WebService.csproj" -c Release -o /app/build

# Test stage
FROM build AS test
WORKDIR /src
RUN dotnet test "tests/WebService.Tests/WebService.Tests.csproj" \
    --configuration Release \
    --logger "trx;LogFileName=test_results.trx" \
    --logger "console;verbosity=detailed" \
    --collect:"XPlat Code Coverage" \
    --results-directory /testresults

# Publish stage
FROM build AS publish
WORKDIR "/src/src/WebService"
RUN dotnet publish "WebService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published application
COPY --from=publish /app/publish .

# Change ownership to non-root user
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Start the application
ENTRYPOINT ["dotnet", "WebService.dll"]
