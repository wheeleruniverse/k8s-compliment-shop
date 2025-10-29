using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using StrawberryShake;
using WebService.Components.Pages;
using WebService.Components.Shared;
using WebService.GraphQL;
using WebService.Services;
using Xunit;

namespace WebService.Tests.Components;

public class HomePageTests : TestContext
{
    private readonly Mock<IComplimentShopClient> _mockGraphQLClient;
    private readonly Mock<IGetProductsQuery> _mockGetProductsQuery;
    private readonly Mock<NavigationManager> _mockNavigationManager;
    private readonly Mock<AnalyticsService> _mockAnalyticsService;

    public HomePageTests()
    {
        _mockGraphQLClient = new Mock<IComplimentShopClient>();
        _mockGetProductsQuery = new Mock<IGetProductsQuery>();
        _mockNavigationManager = new Mock<NavigationManager>();
        _mockAnalyticsService = new Mock<AnalyticsService>(null!, null!);

        _mockGraphQLClient.Setup(c => c.GetProducts).Returns(_mockGetProductsQuery.Object);

        // Register services
        Services.AddSingleton(_mockGraphQLClient.Object);
        Services.AddSingleton(_mockNavigationManager.Object);
        Services.AddSingleton(_mockAnalyticsService.Object);
    }

    [Fact]
    public void HomePage_Renders_WithTitle()
    {
        // Arrange
        SetupSuccessfulProductQuery(new List<Mock<IGetProducts_Products_Items>>());

        // Act
        var cut = RenderComponent<Home>();

        // Assert
        cut.Find("h1.text-title").TextContent.Should().Contain("Premium Compliments");
    }

    [Fact]
    public void HomePage_ShowsSkeletonLoaders_WhileLoading()
    {
        // Arrange
        var taskCompletionSource = new TaskCompletionSource<IOperationResult<IGetProductsResult>>();
        _mockGetProductsQuery
            .Setup(q => q.ExecuteAsync(It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .Returns(taskCompletionSource.Task);

        // Act
        var cut = RenderComponent<Home>();

        // Assert
        var skeletons = cut.FindComponents<SkeletonLoader>();
        skeletons.Count.Should().Be(6);
    }

    [Fact]
    public async Task HomePage_DisplaysProducts_WhenDataLoaded()
    {
        // Arrange
        var products = CreateMockProducts(3);
        SetupSuccessfulProductQuery(products);

        // Act
        var cut = RenderComponent<Home>();
        await Task.Delay(100); // Allow async operations to complete

        // Assert
        var productCards = cut.FindComponents<ProductCard>();
        productCards.Count.Should().Be(3);
    }

    [Fact]
    public async Task HomePage_TracksPageView_OnInitialization()
    {
        // Arrange
        SetupSuccessfulProductQuery(new List<Mock<IGetProducts_Products_Items>>());

        // Act
        var cut = RenderComponent<Home>();
        await Task.Delay(100);

        // Assert
        _mockAnalyticsService.Verify(
            a => a.TrackPageViewAsync("Home - Product Listing", "/"),
            Times.Once);
    }

    [Fact]
    public void HomePage_HasCategoryFilters()
    {
        // Arrange
        SetupSuccessfulProductQuery(new List<Mock<IGetProducts_Products_Items>>());

        // Act
        var cut = RenderComponent<Home>();

        // Assert
        var filterButtons = cut.FindAll("button.btn-glass");
        filterButtons.Count.Should().BeGreaterThanOrEqualTo(4); // All, Appearance, Professional, Personal

        filterButtons.Should().Contain(b => b.TextContent.Contains("All"));
        filterButtons.Should().Contain(b => b.TextContent.Contains("Appearance"));
        filterButtons.Should().Contain(b => b.TextContent.Contains("Professional"));
        filterButtons.Should().Contain(b => b.TextContent.Contains("Personal"));
    }

    [Fact]
    public async Task HomePage_FilterByCategory_LoadsFilteredProducts()
    {
        // Arrange
        var products = CreateMockProducts(3);
        SetupSuccessfulProductQuery(products);

        var cut = RenderComponent<Home>();
        await Task.Delay(100);

        // Act - Click "Appearance" filter
        var appearanceButton = cut.FindAll("button.btn-glass")
            .First(b => b.TextContent.Contains("Appearance"));
        appearanceButton.Click();

        // Assert - Verify query was called with "Appearance" category
        _mockGetProductsQuery.Verify(
            q => q.ExecuteAsync("Appearance", null, null, It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task HomePage_ClickProduct_NavigatesToDetailPage()
    {
        // Arrange
        var products = CreateMockProducts(1);
        products[0].Setup(p => p.Id).Returns(42);
        SetupSuccessfulProductQuery(products);

        var cut = RenderComponent<Home>();
        await Task.Delay(100);

        // Act - Click on the product card
        var productCard = cut.FindComponent<ProductCard>();
        productCard.Find("div.glass-card").Click();

        // Assert
        _mockNavigationManager.Verify(
            n => n.NavigateTo("/product/42", It.IsAny<bool>(), It.IsAny<bool>()),
            Times.Once);
    }

    [Fact]
    public async Task HomePage_ShowsErrorMessage_WhenQueryFails()
    {
        // Arrange
        var errorResult = CreateErrorResult("Failed to load products");
        _mockGetProductsQuery
            .Setup(q => q.ExecuteAsync(It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResult);

        // Act
        var cut = RenderComponent<Home>();
        await Task.Delay(100);

        // Assert
        cut.Markup.Should().Contain("Unable to load products");
        cut.Markup.Should().Contain("Failed to load products");
    }

    [Fact]
    public async Task HomePage_ShowsNoProductsMessage_WhenListIsEmpty()
    {
        // Arrange
        SetupSuccessfulProductQuery(new List<Mock<IGetProducts_Products_Items>>());

        // Act
        var cut = RenderComponent<Home>();
        await Task.Delay(100);

        // Assert
        cut.Markup.Should().Contain("No products found");
    }

    [Fact]
    public async Task HomePage_TryAgainButton_ReloadsProducts()
    {
        // Arrange
        var errorResult = CreateErrorResult("Network error");
        _mockGetProductsQuery
            .Setup(q => q.ExecuteAsync(It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResult);

        var cut = RenderComponent<Home>();
        await Task.Delay(100);

        // Act - Click "Try Again" button
        var tryAgainButton = cut.Find("button.btn-primary");
        tryAgainButton.Click();

        // Assert - Query should be called at least twice (initial + retry)
        _mockGetProductsQuery.Verify(
            q => q.ExecuteAsync(It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()),
            Times.AtLeast(2));
    }

    // Helper methods
    private List<Mock<IGetProducts_Products_Items>> CreateMockProducts(int count)
    {
        var products = new List<Mock<IGetProducts_Products_Items>>();

        for (int i = 1; i <= count; i++)
        {
            var mockProduct = new Mock<IGetProducts_Products_Items>();
            mockProduct.Setup(p => p.Id).Returns(i);
            mockProduct.Setup(p => p.Name).Returns($"Product {i}");
            mockProduct.Setup(p => p.Description).Returns($"Description {i}");
            mockProduct.Setup(p => p.Category).Returns("TestCategory");
            mockProduct.Setup(p => p.CreatedAt).Returns(DateTime.Now.ToString());
            mockProduct.Setup(p => p.UpdatedAt).Returns(DateTime.Now.ToString());

            products.Add(mockProduct);
        }

        return products;
    }

    private void SetupSuccessfulProductQuery(List<Mock<IGetProducts_Products_Items>> products)
    {
        var mockProductConnection = new Mock<IGetProducts_Products>();
        mockProductConnection.Setup(pc => pc.Items).Returns(products.Select(p => p.Object).ToList());
        mockProductConnection.Setup(pc => pc.TotalCount).Returns(products.Count);
        mockProductConnection.Setup(pc => pc.Page).Returns(1);
        mockProductConnection.Setup(pc => pc.PageSize).Returns(20);

        var mockResult = new Mock<IGetProductsResult>();
        mockResult.Setup(r => r.Products).Returns(mockProductConnection.Object);

        var mockOperationResult = new Mock<IOperationResult<IGetProductsResult>>();
        mockOperationResult.Setup(r => r.Data).Returns(mockResult.Object);
        mockOperationResult.Setup(r => r.Errors).Returns((IReadOnlyList<IClientError>?)null);

        _mockGetProductsQuery
            .Setup(q => q.ExecuteAsync(It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockOperationResult.Object);
    }

    private IOperationResult<IGetProductsResult> CreateErrorResult(string errorMessage)
    {
        var mockError = new Mock<IClientError>();
        mockError.Setup(e => e.Message).Returns(errorMessage);

        var mockOperationResult = new Mock<IOperationResult<IGetProductsResult>>();
        mockOperationResult.Setup(r => r.Data).Returns((IGetProductsResult?)null);
        mockOperationResult.Setup(r => r.Errors).Returns(new List<IClientError> { mockError.Object });

        return mockOperationResult.Object;
    }
}
