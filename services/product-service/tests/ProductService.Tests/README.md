# ProductService Unit Tests

Comprehensive unit test suite for the product-service using xUnit, Moq, and FluentAssertions.

## Test Framework

- **xUnit 2.6.2** - Modern, popular .NET test framework
- **Moq 4.20.70** - Mocking library for isolating dependencies
- **FluentAssertions 6.12.0** - Readable assertion library
- **EF Core InMemory 8.0** - In-memory database for repository tests

## Test Coverage

### Product Model Tests (`ProductTests.cs`)
- ✅ JSON-LD conversion with Schema.org compliance
- ✅ Hardcoded price and currency in offers
- ✅ Product URL generation with base URL
- ✅ Default property values
- ✅ Property setters

**Total: 7 tests**

### Product Repository Tests (`ProductRepositoryTests.cs`)
- ✅ GetByIdAsync (valid, invalid, zero, negative IDs)
- ✅ GetAllAsync (no filter, category filter, pagination, combinations)
- ✅ GetTotalCountAsync (with and without filters)
- ✅ CreateAsync (validation, timestamps, count increment)
- ✅ UpdateAsync (updates, timestamp changes, CreatedAt preservation)
- ✅ DeleteAsync (success, failure, isolation)

**Total: 24 tests**

### Product gRPC Service Tests (`ProductGrpcServiceTests.cs`)
- ✅ GetProduct (success, not found)
- ✅ ListProducts (all, filtered, paginated, defaults)
- ✅ CreateProduct (success, repository verification)
- ✅ UpdateProduct (success, not found)
- ✅ DeleteProduct (success, failure)
- ✅ GetProductJsonLd (success, not found)

**Total: 14 tests**

## Running Tests

### Run All Tests

```bash
# From solution root
cd /Users/justin.wheeler/Downloads/k8s-compliment-shop/services/product-service
dotnet test

# Or from test project
cd tests/ProductService.Tests
dotnet test
```

### Run with Detailed Output

```bash
dotnet test --verbosity normal
```

### Run Specific Test Class

```bash
dotnet test --filter "FullyQualifiedName~ProductTests"
dotnet test --filter "FullyQualifiedName~ProductRepositoryTests"
dotnet test --filter "FullyQualifiedName~ProductGrpcServiceTests"
```

### Run with Code Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Test Organization

```
tests/ProductService.Tests/
├── Unit/
│   ├── Models/
│   │   └── ProductTests.cs
│   ├── Repositories/
│   │   └── ProductRepositoryTests.cs
│   └── Services/
│       └── ProductGrpcServiceTests.cs
├── Helpers/
│   └── DbContextFactory.cs
├── ProductService.Tests.csproj
└── README.md
```

## Testing Patterns

### Repository Tests
- Use **EF Core InMemory** provider for fast, isolated database tests
- Each test gets a fresh database instance via `DbContextFactory`
- Tests verify both return values and database state

### Service Tests
- Use **Moq** to mock `IProductRepository` and `ILogger`
- Test business logic in isolation from data layer
- Verify gRPC responses and exception handling

### Model Tests
- Direct unit tests without mocking
- Focus on business logic (JSON-LD conversion)
- Validate default values and property behavior

## Example Test Output

```
Passed!  - Failed:     0, Passed:    45, Skipped:     0, Total:    45
```

## Benefits

1. **Fast Feedback** - Tests run in milliseconds
2. **Regression Prevention** - Catch bugs before deployment
3. **Refactoring Confidence** - Safely change code
4. **Living Documentation** - Tests describe expected behavior
5. **CI/CD Ready** - Can be integrated into build pipelines

## Continuous Integration

Add to your CI pipeline:

```yaml
- name: Run Tests
  run: dotnet test --logger "trx;LogFileName=test-results.trx"

- name: Publish Test Results
  uses: dorny/test-reporter@v1
  if: always()
  with:
    name: .NET Tests
    path: '**/test-results.trx'
    reporter: dotnet-trx
```

## Next Steps

- [ ] Add integration tests with real MySQL
- [ ] Add gRPC client integration tests
- [ ] Measure code coverage (target: >80%)
- [ ] Add performance/benchmark tests
- [ ] Add mutation testing with Stryker.NET
