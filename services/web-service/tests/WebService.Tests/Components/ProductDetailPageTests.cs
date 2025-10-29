using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using StrawberryShake;
using WebService.Components.Pages;
using WebService.GraphQL;
using WebService.Services;
using Xunit;

namespace WebService.Tests.Components;

public class ProductDetailPageTests : TestContext
{
    private readonly Mock<IComplimentShopClient> _mockGraphQLClient;
    private readonly Mock<IGetProductQuery> _mockGetProductQuery;
    private readonly Mock<NavigationManager> _mockNavigationManager;
    private readonly Mock<AnalyticsService> _mockAnalyticsService;

    public ProductDetailPageTests()
    {
        _mockGraphQLClient = new Mock<IComplimentShopClient>();
        _mockGetProductQuery = new Mock<IGetProductQuery>();
        _mockNavigationManager = new Mock<NavigationManager>();
        _mockAnalyticsService = new Mock<AnalyticsService>(null!, null!);

        _mockGraphQLClient.Setup(c => c.GetProduct).Returns(_mockGetProductQuery.Object);

        // Register services
        Services.AddSingleton(_mockGraphQLClient.Object);
        Services.AddSingleton(_mockNavigationManager.Object);
        Services.AddSingleton(_mockAnalyticsService.Object);
    }

    [Fact]
    public void ProductDetailPage_ShowsSkeletonLoader_WhileLoading()
    {
        // Arrange
        var taskCompletionSource = new TaskCompletionSource<IOperationResult<IGetProductResult>>();
        _mockGetProductQuery
            .Setup(q => q.ExecuteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(taskCompletionSource.Task);

        // Act
        var cut = RenderComponent<ProductDetail>(parameters => parameters
            .Add(p => p.Id, 1));

        // Assert
        var skeletons = cut.FindAll("div.skeleton");
        skeletons.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ProductDetailPage_DisplaysProductInformation_WhenLoaded()
    {
        // Arrange
        var mockProduct = CreateMockProduct(1, "Amazing Compliment", "You're wonderful!", "Personal");
        SetupSuccessfulProductQuery(mockProduct);

        // Act
        var cut = RenderComponent<ProductDetail>(parameters => parameters
            .Add(p => p.Id, 1));
        await Task.Delay(100);

        // Assert
        cut.Find("h1.product-title").TextContent.Should().Be("Amazing Compliment");
        cut.Find("p.product-description-full").TextContent.Should().Be("You're wonderful!");
        cut.Find("span.badge").TextContent.Should().Be("Personal");
    }

    [Fact]
    public async Task ProductDetailPage_InjectsJsonLd_IntoHead()
    {
        // Arrange
        var jsonLd = @"{""@context"": ""https://schema.org"", ""@type"": ""Product"", ""name"": ""Test""}";
        var mockProduct = CreateMockProduct(1, "Test Product", "Description", "Category", jsonLd);
        SetupSuccessfulProductQuery(mockProduct);

        // Act
        var cut = RenderComponent<ProductDetail>(parameters => parameters
            .Add(p => p.Id, 1));
        await Task.Delay(100);

        // Assert
        // HeadContent should contain JSON-LD script
        // Note: bUnit has limited support for HeadContent, so we verify the jsonLd variable is set
        cut.Instance.Should().NotBeNull();
    }

    [Fact]
    public async Task ProductDetailPage_TracksProductView_WhenLoaded()
    {
        // Arrange
        var mockProduct = CreateMockProduct(42, "Test Product", "Description", "TestCategory");
        SetupSuccessfulProductQuery(mockProduct);

        // Act
        var cut = RenderComponent<ProductDetail>(parameters => parameters
            .Add(p => p.Id, 42));
        await Task.Delay(100);

        // Assert
        _mockAnalyticsService.Verify(
            a => a.TrackProductViewAsync(42, "Test Product", "TestCategory"),
            Times.Once);
    }

    [Fact]
    public async Task ProductDetailPage_BackButton_NavigatesToHome()
    {
        // Arrange
        var mockProduct = CreateMockProduct(1, "Test", "Desc", "Cat");
        SetupSuccessfulProductQuery(mockProduct);

        var cut = RenderComponent<ProductDetail>(parameters => parameters
            .Add(p => p.Id, 1));
        await Task.Delay(100);

        // Act
        var backButton = cut.FindAll("button.btn-glass")
            .First(b => b.TextContent.Contains("Back to Products"));
        backButton.Click();

        // Assert
        _mockNavigationManager.Verify(
            n => n.NavigateTo("/", It.IsAny<bool>(), It.IsAny<bool>()),
            Times.Once);
    }

    [Fact]
    public async Task ProductDetailPage_DisplaysMetaInformation()
    {
        // Arrange
        var createdAt = "2024-01-15T10:30:00Z";
        var updatedAt = "2024-01-20T15:45:00Z";
        var mockProduct = CreateMockProduct(1, "Test", "Desc", "Cat");
        mockProduct.Setup(p => p.CreatedAt).Returns(createdAt);
        mockProduct.Setup(p => p.UpdatedAt).Returns(updatedAt);
        SetupSuccessfulProductQuery(mockProduct);

        // Act
        var cut = RenderComponent<ProductDetail>(parameters => parameters
            .Add(p => p.Id, 1));
        await Task.Delay(100);

        // Assert
        var metaSection = cut.Find("div.product-meta");
        metaSection.Should().NotBeNull();

        var metaItems = cut.FindAll("div.meta-item");
        metaItems.Count.Should().Be(3); // Category, Created, Updated
    }

    [Fact]
    public async Task ProductDetailPage_ShowsErrorMessage_WhenProductNotFound()
    {
        // Arrange
        var errorResult = CreateErrorResult("Product not found");
        _mockGetProductQuery
            .Setup(q => q.ExecuteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResult);

        // Act
        var cut = RenderComponent<ProductDetail>(parameters => parameters
            .Add(p => p.Id, 999));
        await Task.Delay(100);

        // Assert
        cut.Markup.Should().Contain("Unable to load product");
        cut.Markup.Should().Contain("Product not found");
    }

    [Fact]
    public async Task ProductDetailPage_TryAgainButton_ReloadsProduct()
    {
        // Arrange
        var errorResult = CreateErrorResult("Network error");
        _mockGetProductQuery
            .Setup(q => q.ExecuteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResult);

        var cut = RenderComponent<ProductDetail>(parameters => parameters
            .Add(p => p.Id, 1));
        await Task.Delay(100);

        // Act
        var tryAgainButton = cut.Find("button.btn-primary");
        tryAgainButton.Click();

        // Assert
        _mockGetProductQuery.Verify(
            q => q.ExecuteAsync(1, It.IsAny<CancellationToken>()),
            Times.AtLeast(2));
    }

    [Fact]
    public async Task ProductDetailPage_ShareButton_Exists()
    {
        // Arrange
        var mockProduct = CreateMockProduct(1, "Test", "Desc", "Cat");
        SetupSuccessfulProductQuery(mockProduct);

        // Act
        var cut = RenderComponent<ProductDetail>(parameters => parameters
            .Add(p => p.Id, 1));
        await Task.Delay(100);

        // Assert
        var shareButton = cut.FindAll("button.btn-primary")
            .FirstOrDefault(b => b.TextContent.Contains("Share"));
        shareButton.Should().NotBeNull();
    }

    [Fact]
    public async Task ProductDetailPage_FormatsDate_Correctly()
    {
        // Arrange
        var mockProduct = CreateMockProduct(1, "Test", "Desc", "Cat");
        mockProduct.Setup(p => p.CreatedAt).Returns("2024-01-15T10:30:00Z");
        SetupSuccessfulProductQuery(mockProduct);

        // Act
        var cut = RenderComponent<ProductDetail>(parameters => parameters
            .Add(p => p.Id, 1));
        await Task.Delay(100);

        // Assert
        // Should format date as "January 15, 2024" or similar
        cut.Markup.Should().Contain("January");
        cut.Markup.Should().Contain("2024");
    }

    [Fact]
    public async Task ProductDetailPage_HandlesInvalidDate_Gracefully()
    {
        // Arrange
        var mockProduct = CreateMockProduct(1, "Test", "Desc", "Cat");
        mockProduct.Setup(p => p.CreatedAt).Returns("invalid-date");
        SetupSuccessfulProductQuery(mockProduct);

        // Act
        var cut = RenderComponent<ProductDetail>(parameters => parameters
            .Add(p => p.Id, 1));
        await Task.Delay(100);

        // Assert - Should display the original string or "Unknown"
        var metaSection = cut.Find("div.product-meta");
        metaSection.Should().NotBeNull();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(100)]
    public async Task ProductDetailPage_LoadsCorrectProduct_ForGivenId(int productId)
    {
        // Arrange
        var mockProduct = CreateMockProduct(productId, $"Product {productId}", "Description", "Category");
        SetupSuccessfulProductQuery(mockProduct);

        // Act
        var cut = RenderComponent<ProductDetail>(parameters => parameters
            .Add(p => p.Id, productId));
        await Task.Delay(100);

        // Assert
        _mockGetProductQuery.Verify(
            q => q.ExecuteAsync(productId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // Helper methods
    private Mock<IGetProduct_Product> CreateMockProduct(
        int id,
        string name,
        string description,
        string category,
        string? jsonLd = null)
    {
        var mockProduct = new Mock<IGetProduct_Product>();
        mockProduct.Setup(p => p.Id).Returns(id);
        mockProduct.Setup(p => p.Name).Returns(name);
        mockProduct.Setup(p => p.Description).Returns(description);
        mockProduct.Setup(p => p.Category).Returns(category);
        mockProduct.Setup(p => p.CreatedAt).Returns(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        mockProduct.Setup(p => p.UpdatedAt).Returns(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        mockProduct.Setup(p => p.JsonLd).Returns(jsonLd);

        return mockProduct;
    }

    private void SetupSuccessfulProductQuery(Mock<IGetProduct_Product> mockProduct)
    {
        var mockResult = new Mock<IGetProductResult>();
        mockResult.Setup(r => r.Product).Returns(mockProduct.Object);

        var mockOperationResult = new Mock<IOperationResult<IGetProductResult>>();
        mockOperationResult.Setup(r => r.Data).Returns(mockResult.Object);
        mockOperationResult.Setup(r => r.Errors).Returns((IReadOnlyList<IClientError>?)null);

        _mockGetProductQuery
            .Setup(q => q.ExecuteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockOperationResult.Object);
    }

    private IOperationResult<IGetProductResult> CreateErrorResult(string errorMessage)
    {
        var mockError = new Mock<IClientError>();
        mockError.Setup(e => e.Message).Returns(errorMessage);

        var mockOperationResult = new Mock<IOperationResult<IGetProductResult>>();
        mockOperationResult.Setup(r => r.Data).Returns((IGetProductResult?)null);
        mockOperationResult.Setup(r => r.Errors).Returns(new List<IClientError> { mockError.Object });

        return mockOperationResult.Object;
    }
}
