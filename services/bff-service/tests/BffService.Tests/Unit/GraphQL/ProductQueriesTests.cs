using BffService.GraphQL.Queries;
using BffService.Services;
using FluentAssertions;
using Moq;
using ProductService.Protos;
using Xunit;

namespace BffService.Tests.Unit.GraphQL;

public class ProductQueriesTests
{
    private readonly Mock<IProductServiceClient> _mockProductService;
    private readonly ProductQueries _queries;

    public ProductQueriesTests()
    {
        _mockProductService = new Mock<IProductServiceClient>();
        _queries = new ProductQueries();
    }

    #region GetProductAsync Tests

    [Fact]
    public async Task GetProductAsync_WithValidId_ShouldReturnProduct()
    {
        // Arrange
        var productResponse = new ProductResponse
        {
            Id = 1,
            Name = "Test Product",
            Description = "Test Description",
            Category = "Test",
            CreatedAt = "2024-01-01T00:00:00Z",
            UpdatedAt = "2024-01-01T00:00:00Z"
        };

        _mockProductService
            .Setup(x => x.GetProductAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(productResponse);

        // Act
        var result = await _queries.GetProductAsync(_mockProductService.Object, 1, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test Product");
        result.Description.Should().Be("Test Description");
        result.Category.Should().Be("Test");
    }

    [Fact]
    public async Task GetProductAsync_WithNotFoundId_ShouldReturnNull()
    {
        // Arrange
        _mockProductService
            .Setup(x => x.GetProductAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductResponse?)null);

        // Act
        var result = await _queries.GetProductAsync(_mockProductService.Object, 999, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetProductsAsync Tests

    [Fact]
    public async Task GetProductsAsync_WithoutCategory_ShouldReturnAllProducts()
    {
        // Arrange
        var listResponse = new ListProductsResponse
        {
            TotalCount = 2,
            Page = 1,
            PageSize = 20
        };
        listResponse.Products.Add(new ProductResponse { Id = 1, Name = "Product 1", Description = "Desc 1", Category = "Cat1", CreatedAt = "2024-01-01T00:00:00Z", UpdatedAt = "2024-01-01T00:00:00Z" });
        listResponse.Products.Add(new ProductResponse { Id = 2, Name = "Product 2", Description = "Desc 2", Category = "Cat2", CreatedAt = "2024-01-01T00:00:00Z", UpdatedAt = "2024-01-01T00:00:00Z" });

        _mockProductService
            .Setup(x => x.ListProductsAsync(null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(listResponse);

        // Act
        var result = await _queries.GetProductsAsync(_mockProductService.Object, null, 1, 20, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task GetProductsAsync_WithCategory_ShouldReturnFilteredProducts()
    {
        // Arrange
        var listResponse = new ListProductsResponse
        {
            TotalCount = 1,
            Page = 1,
            PageSize = 20
        };
        listResponse.Products.Add(new ProductResponse { Id = 1, Name = "Product 1", Description = "Desc 1", Category = "Appearance", CreatedAt = "2024-01-01T00:00:00Z", UpdatedAt = "2024-01-01T00:00:00Z" });

        _mockProductService
            .Setup(x => x.ListProductsAsync("Appearance", 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(listResponse);

        // Act
        var result = await _queries.GetProductsAsync(_mockProductService.Object, "Appearance", 1, 20, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items.First().Category.Should().Be("Appearance");
    }

    [Fact]
    public async Task GetProductsAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var listResponse = new ListProductsResponse
        {
            TotalCount = 10,
            Page = 2,
            PageSize = 5
        };

        _mockProductService
            .Setup(x => x.ListProductsAsync(null, 2, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(listResponse);

        // Act
        var result = await _queries.GetProductsAsync(_mockProductService.Object, null, 2, 5, CancellationToken.None);

        // Assert
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(10);
    }

    #endregion
}
