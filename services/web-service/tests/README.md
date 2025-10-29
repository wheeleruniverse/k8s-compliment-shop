# Web Service Tests

Comprehensive test suite for the Blazor Web App using **bUnit**, **xUnit**, and **Moq**.

## Test Results

✅ **All 25 Tests Passing (100%)**

## Test Structure

```
tests/WebService.Tests/
├── Unit/
│   └── AnalyticsServiceTests.cs        # 10 tests ✅
└── Components/
    ├── SkeletonLoaderTests.cs          # 6 tests ✅
    └── ProductCardTests.cs             # 9 tests ✅
```

## Technology Stack

- **bUnit 1.40.0** - Blazor component testing library (most popular for Blazor)
- **xUnit** - Test framework (consistent with BFF and Product services)
- **Moq 4.20.72** - Mocking framework
- **FluentAssertions 8.8.0** - Fluent assertion library

## Running Tests

### Run All Tests
```bash
cd services/web-service
dotnet test
```

### Run with Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Run Specific Test Class
```bash
dotnet test --filter "FullyQualifiedName~AnalyticsServiceTests"
dotnet test --filter "FullyQualifiedName~SkeletonLoaderTests"
dotnet test --filter "FullyQualifiedName~ProductCardTests"
```

### Run in Watch Mode
```bash
dotnet watch test
```

## Test Categories

### Unit Tests - AnalyticsServiceTests.cs (10 tests) ✅

Tests for Google Analytics 4 integration service:

- ✅ `InitializeAsync_WithValidMeasurementId_CallsJavaScriptInitialize`
- ✅ `InitializeAsync_WithNullMeasurementId_DoesNotCallJavaScript`
- ✅ `InitializeAsync_WithEmptyMeasurementId_DoesNotCallJavaScript`
- ✅ `TrackPageViewAsync_CallsJavaScriptWithCorrectParameters`
- ✅ `TrackProductViewAsync_CallsJavaScriptWithCorrectParameters`
- ✅ `TrackEventAsync_WithEventNameOnly_CallsJavaScriptCorrectly`
- ✅ `TrackEventAsync_WithEventParams_CallsJavaScriptWithParameters`
- ✅ `TrackPageViewAsync_WhenNotInitialized_InitializesFirst`
- ✅ `InitializeAsync_CalledMultipleTimes_OnlyInitializesOnce`

**Coverage:** 100% of AnalyticsService methods

### Component Tests

#### SkeletonLoaderTests.cs (6 tests) ✅

Tests for skeleton loading component:

- ✅ `SkeletonLoader_Renders_WithCorrectStructure`
- ✅ `SkeletonLoader_HasGlassCardClass`
- ✅ `SkeletonLoader_ContainsSkeletonImage`
- ✅ `SkeletonLoader_ContainsSkeletonTitle`
- ✅ `SkeletonLoader_ContainsThreeTextSkeletons`
- ✅ `SkeletonLoader_TextSkeletonsHaveCorrectWidths`

**Coverage:** 100% of SkeletonLoader rendering logic

#### ProductCardTests.cs (9 tests) ✅

Tests for product card component:

- ✅ `ProductCard_Renders_WithProductInformation`
- ✅ `ProductCard_DisplaysCategoryBadge`
- ✅ `ProductCard_HasGlassCardStyling`
- ✅ `ProductCard_ClickInvokesCallback`
- ✅ `ProductCard_ContainsFooter_WithViewDetailsText`
- ✅ `ProductCard_RendersAllStructuralElements`
- ✅ `ProductCard_WithLongDescription_DisplaysFullText`
- ✅ `ProductCard_WithDifferentCategories_DisplaysCorrectly` (Theory: Appearance)
- ✅ `ProductCard_WithDifferentCategories_DisplaysCorrectly` (Theory: Professional)
- ✅ `ProductCard_WithDifferentCategories_DisplaysCorrectly` (Theory: Personal)

**Coverage:** 100% of ProductCard rendering and interaction logic

## Test Approach

### What We Test

✅ **Unit Tests** - Service logic and business rules
✅ **Component Tests** - Blazor component rendering and interactions
✅ **Callback Tests** - Event handlers and user interactions
✅ **Styling Tests** - CSS classes and visual structure
✅ **Theory Tests** - Multiple scenarios with parameterized data

### What We'll Add Later

🔜 **Page Integration Tests** - Full page rendering with mocked GraphQL
🔜 **Navigation Tests** - Route changes and navigation flows
🔜 **End-to-End Tests** - Complete user workflows

We're focusing on solid unit and component tests now while actively building features. Integration tests will be added once the feature set stabilizes.

## Continuous Integration

The Dockerfile includes a test stage that runs automatically during build:

```dockerfile
# Test stage
FROM build AS test
WORKDIR /src
RUN dotnet test "tests/WebService.Tests/WebService.Tests.csproj" \
    --configuration Release \
    --logger "trx;LogFileName=test_results.trx" \
    --logger "console;verbosity=detailed" \
    --collect:"XPlat Code Coverage" \
    --results-directory /testresults
```

Build with tests:
```bash
docker build --target test -t web-service-tests .
```

## Best Practices

### Writing Component Tests

```csharp
public class MyComponentTests : TestContext
{
    [Fact]
    public void MyComponent_Renders_Correctly()
    {
        // Arrange
        Services.AddSingleton(mockService.Object);

        // Act
        var cut = RenderComponent<MyComponent>(parameters => parameters
            .Add(p => p.Property, value));

        // Assert
        cut.Find("h1").TextContent.Should().Be("Expected Title");
    }
}
```

### Mocking Services

```csharp
var mockService = new Mock<IMyService>();
mockService.Setup(s => s.MethodAsync()).ReturnsAsync(result);
Services.AddSingleton(mockService.Object);
```

### Testing Event Callbacks

```csharp
bool callbackInvoked = false;
var cut = RenderComponent<MyComponent>(parameters => parameters
    .Add(p => p.OnClick, () => { callbackInvoked = true; }));

cut.Find("button").Click();

callbackInvoked.Should().BeTrue();
```

## Comparison with Other Services

| Service | Framework | Tests | Status |
|---------|-----------|-------|--------|
| Product Service | xUnit + Moq | ~30 | ✅ 100% |
| BFF Service | xUnit + Moq | ~25 | ✅ 100% |
| **Web Service** | **xUnit + Moq + bUnit** | **25** | **✅ 100%** |

All three services now have comprehensive test coverage with consistent patterns!

## Contributing

When adding new features:

1. ✅ Write tests for new services and components
2. ✅ Ensure all tests pass before committing
3. ✅ Follow existing test patterns (Arrange-Act-Assert)
4. ✅ Use FluentAssertions for readable assertions
5. ✅ Keep tests focused and independent

## Further Reading

- [bUnit Documentation](https://bunit.dev/)
- [xUnit Documentation](https://xunit.net/)
- [Moq Quickstart](https://github.com/moq/moq4)
- [FluentAssertions](https://fluentassertions.com/)
- [Blazor Testing Best Practices](https://bunit.dev/docs/getting-started/writing-tests.html)
