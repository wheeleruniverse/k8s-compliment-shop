using BffService.Services;
using FluentAssertions;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;
using ProductService.Protos;
using Xunit;

namespace BffService.Tests.Unit.Services;

public class ProductServiceClientTests
{
    private readonly Mock<ProductService.Protos.ProductService.ProductServiceClient> _mockGrpcClient;
    private readonly Mock<ILogger<ProductServiceClient>> _mockLogger;
    private readonly ProductServiceClient _client;

    public ProductServiceClientTests()
    {
        _mockGrpcClient = new Mock<ProductService.Protos.ProductService.ProductServiceClient>();
        _mockLogger = new Mock<ILogger<ProductServiceClient>>();
        _client = new ProductServiceClient(_mockGrpcClient.Object, _mockLogger.Object);
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

        _mockGrpcClient
            .Setup(x => x.GetProductAsync(
                It.Is<GetProductRequest>(r => r.Id == 1),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .Returns(CreateAsyncUnaryCall(productResponse));

        // Act
        var result = await _client.GetProductAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task GetProductAsync_WithNotFoundId_ShouldReturnNull()
    {
        // Arrange
        _mockGrpcClient
            .Setup(x => x.GetProductAsync(
                It.Is<GetProductRequest>(r => r.Id == 999),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .Throws(new RpcException(new Status(StatusCode.NotFound, "Not found")));

        // Act
        var result = await _client.GetProductAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region ListProductsAsync Tests

    [Fact]
    public async Task ListProductsAsync_WithoutCategory_ShouldReturnAllProducts()
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

        _mockGrpcClient
            .Setup(x => x.ListProductsAsync(
                It.Is<ListProductsRequest>(r => r.Category == "" && r.Page == 1 && r.PageSize == 20),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .Returns(CreateAsyncUnaryCall(listResponse));

        // Act
        var result = await _client.ListProductsAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.Products.Should().HaveCount(2);
    }

    [Fact]
    public async Task ListProductsAsync_WithCategory_ShouldReturnFilteredProducts()
    {
        // Arrange
        var listResponse = new ListProductsResponse
        {
            TotalCount = 1,
            Page = 1,
            PageSize = 20
        };
        listResponse.Products.Add(new ProductResponse { Id = 1, Name = "Product 1", Description = "Desc 1", Category = "Appearance", CreatedAt = "2024-01-01T00:00:00Z", UpdatedAt = "2024-01-01T00:00:00Z" });

        _mockGrpcClient
            .Setup(x => x.ListProductsAsync(
                It.Is<ListProductsRequest>(r => r.Category == "Appearance"),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .Returns(CreateAsyncUnaryCall(listResponse));

        // Act
        var result = await _client.ListProductsAsync("Appearance");

        // Assert
        result.TotalCount.Should().Be(1);
        result.Products.Should().HaveCount(1);
        result.Products.First().Category.Should().Be("Appearance");
    }

    [Fact]
    public async Task ListProductsAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var listResponse = new ListProductsResponse
        {
            TotalCount = 10,
            Page = 2,
            PageSize = 5
        };

        _mockGrpcClient
            .Setup(x => x.ListProductsAsync(
                It.Is<ListProductsRequest>(r => r.Page == 2 && r.PageSize == 5),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .Returns(CreateAsyncUnaryCall(listResponse));

        // Act
        var result = await _client.ListProductsAsync(null, 2, 5);

        // Assert
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(10);
    }

    #endregion

    #region GetProductJsonLdAsync Tests

    [Fact]
    public async Task GetProductJsonLdAsync_WithValidId_ShouldReturnJsonLd()
    {
        // Arrange
        var jsonLdResponse = new ProductJsonLdResponse
        {
            JsonLd = "{\"@context\":\"https://schema.org\"}"
        };

        _mockGrpcClient
            .Setup(x => x.GetProductJsonLdAsync(
                It.Is<GetProductRequest>(r => r.Id == 1),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .Returns(CreateAsyncUnaryCall(jsonLdResponse));

        // Act
        var result = await _client.GetProductJsonLdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("@context");
    }

    [Fact]
    public async Task GetProductJsonLdAsync_WithNotFoundId_ShouldReturnNull()
    {
        // Arrange
        _mockGrpcClient
            .Setup(x => x.GetProductJsonLdAsync(
                It.Is<GetProductRequest>(r => r.Id == 999),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .Throws(new RpcException(new Status(StatusCode.NotFound, "Not found")));

        // Act
        var result = await _client.GetProductJsonLdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateProductAsync Tests

    [Fact]
    public async Task CreateProductAsync_WithValidData_ShouldReturnCreatedProduct()
    {
        // Arrange
        var createdProduct = new ProductResponse
        {
            Id = 7,
            Name = "New Product",
            Description = "New Description",
            Category = "New",
            CreatedAt = "2024-01-01T00:00:00Z",
            UpdatedAt = "2024-01-01T00:00:00Z"
        };

        _mockGrpcClient
            .Setup(x => x.CreateProductAsync(
                It.Is<CreateProductRequest>(r => r.Name == "New Product"),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .Returns(CreateAsyncUnaryCall(createdProduct));

        // Act
        var result = await _client.CreateProductAsync("New Product", "New Description", "New");

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(7);
        result.Name.Should().Be("New Product");
    }

    #endregion

    #region UpdateProductAsync Tests

    [Fact]
    public async Task UpdateProductAsync_WithValidData_ShouldReturnUpdatedProduct()
    {
        // Arrange
        var updatedProduct = new ProductResponse
        {
            Id = 1,
            Name = "Updated Product",
            Description = "Updated Description",
            Category = "Updated",
            CreatedAt = "2024-01-01T00:00:00Z",
            UpdatedAt = "2024-01-02T00:00:00Z"
        };

        _mockGrpcClient
            .Setup(x => x.UpdateProductAsync(
                It.Is<UpdateProductRequest>(r => r.Id == 1 && r.Name == "Updated Product"),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .Returns(CreateAsyncUnaryCall(updatedProduct));

        // Act
        var result = await _client.UpdateProductAsync(1, "Updated Product", "Updated Description", "Updated");

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Updated Product");
    }

    #endregion

    #region DeleteProductAsync Tests

    [Fact]
    public async Task DeleteProductAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        var deleteResponse = new DeleteProductResponse
        {
            Success = true,
            Message = "Product deleted"
        };

        _mockGrpcClient
            .Setup(x => x.DeleteProductAsync(
                It.Is<DeleteProductRequest>(r => r.Id == 1),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .Returns(CreateAsyncUnaryCall(deleteResponse));

        // Act
        var result = await _client.DeleteProductAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteProductAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var deleteResponse = new DeleteProductResponse
        {
            Success = false,
            Message = "Product not found"
        };

        _mockGrpcClient
            .Setup(x => x.DeleteProductAsync(
                It.Is<DeleteProductRequest>(r => r.Id == 999),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .Returns(CreateAsyncUnaryCall(deleteResponse));

        // Act
        var result = await _client.DeleteProductAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    /// <summary>
    /// Helper method to create AsyncUnaryCall for mocking gRPC responses
    /// </summary>
    private static AsyncUnaryCall<TResponse> CreateAsyncUnaryCall<TResponse>(TResponse response)
    {
        return new AsyncUnaryCall<TResponse>(
            Task.FromResult(response),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });
    }
}
