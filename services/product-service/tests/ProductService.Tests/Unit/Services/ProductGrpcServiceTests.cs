using FluentAssertions;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;
using ProductService.Models;
using ProductService.Protos;
using ProductService.Repositories;
using ProductService.Services;
using Xunit;

namespace ProductService.Tests.Unit.Services;

public class ProductGrpcServiceTests
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly Mock<ILogger<ProductGrpcService>> _mockLogger;
    private readonly ProductGrpcService _service;

    public ProductGrpcServiceTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _mockLogger = new Mock<ILogger<ProductGrpcService>>();
        _service = new ProductGrpcService(_mockRepository.Object, _mockLogger.Object);
    }

    #region GetProduct Tests

    [Fact]
    public async Task GetProduct_WithValidId_ShouldReturnProduct()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Test Compliment",
            Description = "A test description",
            Category = "Test",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        var request = new GetProductRequest { Id = 1 };
        var context = TestServerCallContext.Create();

        // Act
        var result = await _service.GetProduct(request, context);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Test Compliment");
        result.Description.Should().Be("A test description");
        result.Category.Should().Be("Test");
    }

    [Fact]
    public async Task GetProduct_WithInvalidId_ShouldThrowRpcException()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        var request = new GetProductRequest { Id = 999 };
        var context = TestServerCallContext.Create();

        // Act
        Func<Task> act = async () => await _service.GetProduct(request, context);

        // Assert
        await act.Should().ThrowAsync<RpcException>()
            .Where(e => e.StatusCode == StatusCode.NotFound);
    }

    #endregion

    #region ListProducts Tests

    [Fact]
    public async Task ListProducts_WithoutFilter_ShouldReturnAllProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Description = "Desc 1", Category = "Cat1", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Product 2", Description = "Desc 2", Category = "Cat2", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        _mockRepository.Setup(r => r.GetAllAsync(null, 1, 20)).ReturnsAsync(products);
        _mockRepository.Setup(r => r.GetTotalCountAsync(null)).ReturnsAsync(2);

        var request = new ListProductsRequest();
        var context = TestServerCallContext.Create();

        // Act
        var result = await _service.ListProducts(request, context);

        // Assert
        result.Should().NotBeNull();
        result.Products.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task ListProducts_WithCategoryFilter_ShouldReturnFilteredProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Description = "Desc 1", Category = "Appearance", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        _mockRepository.Setup(r => r.GetAllAsync("Appearance", 1, 20)).ReturnsAsync(products);
        _mockRepository.Setup(r => r.GetTotalCountAsync("Appearance")).ReturnsAsync(1);

        var request = new ListProductsRequest { Category = "Appearance" };
        var context = TestServerCallContext.Create();

        // Act
        var result = await _service.ListProducts(request, context);

        // Assert
        result.Products.Should().HaveCount(1);
        result.Products.First().Category.Should().Be("Appearance");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task ListProducts_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 3, Name = "Product 3", Description = "Desc 3", Category = "Cat3", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        _mockRepository.Setup(r => r.GetAllAsync(null, 2, 5)).ReturnsAsync(products);
        _mockRepository.Setup(r => r.GetTotalCountAsync(null)).ReturnsAsync(10);

        var request = new ListProductsRequest { Page = 2, PageSize = 5 };
        var context = TestServerCallContext.Create();

        // Act
        var result = await _service.ListProducts(request, context);

        // Assert
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(10);
    }

    [Fact]
    public async Task ListProducts_WithInvalidPage_ShouldDefaultToPageOne()
    {
        // Arrange
        var products = new List<Product>();
        _mockRepository.Setup(r => r.GetAllAsync(null, 1, 20)).ReturnsAsync(products);
        _mockRepository.Setup(r => r.GetTotalCountAsync(null)).ReturnsAsync(0);

        var request = new ListProductsRequest { Page = 0 };
        var context = TestServerCallContext.Create();

        // Act
        var result = await _service.ListProducts(request, context);

        // Assert
        result.Page.Should().Be(1);
    }

    #endregion

    #region CreateProduct Tests

    [Fact]
    public async Task CreateProduct_WithValidRequest_ShouldCreateProduct()
    {
        // Arrange
        var createdProduct = new Product
        {
            Id = 7,
            Name = "New Compliment",
            Description = "A new description",
            Category = "New",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Product>())).ReturnsAsync(createdProduct);

        var request = new CreateProductRequest
        {
            Name = "New Compliment",
            Description = "A new description",
            Category = "New"
        };
        var context = TestServerCallContext.Create();

        // Act
        var result = await _service.CreateProduct(request, context);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(7);
        result.Name.Should().Be("New Compliment");
        result.Description.Should().Be("A new description");
        result.Category.Should().Be("New");
    }

    [Fact]
    public async Task CreateProduct_ShouldCallRepositoryWithCorrectData()
    {
        // Arrange
        var createdProduct = new Product
        {
            Id = 7,
            Name = "New Compliment",
            Description = "A new description",
            Category = "New",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Product>())).ReturnsAsync(createdProduct);

        var request = new CreateProductRequest
        {
            Name = "New Compliment",
            Description = "A new description",
            Category = "New"
        };
        var context = TestServerCallContext.Create();

        // Act
        await _service.CreateProduct(request, context);

        // Assert
        _mockRepository.Verify(r => r.CreateAsync(It.Is<Product>(p =>
            p.Name == "New Compliment" &&
            p.Description == "A new description" &&
            p.Category == "New"
        )), Times.Once);
    }

    #endregion

    #region UpdateProduct Tests

    [Fact]
    public async Task UpdateProduct_WithValidId_ShouldUpdateProduct()
    {
        // Arrange
        var existingProduct = new Product
        {
            Id = 1,
            Name = "Old Name",
            Description = "Old Description",
            Category = "Old",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var updatedProduct = new Product
        {
            Id = 1,
            Name = "Updated Name",
            Description = "Updated Description",
            Category = "Updated",
            CreatedAt = existingProduct.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingProduct);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Product>())).ReturnsAsync(updatedProduct);

        var request = new UpdateProductRequest
        {
            Id = 1,
            Name = "Updated Name",
            Description = "Updated Description",
            Category = "Updated"
        };
        var context = TestServerCallContext.Create();

        // Act
        var result = await _service.UpdateProduct(request, context);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Description");
        result.Category.Should().Be("Updated");
    }

    [Fact]
    public async Task UpdateProduct_WithInvalidId_ShouldThrowRpcException()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        var request = new UpdateProductRequest
        {
            Id = 999,
            Name = "Test",
            Description = "Test",
            Category = "Test"
        };
        var context = TestServerCallContext.Create();

        // Act
        Func<Task> act = async () => await _service.UpdateProduct(request, context);

        // Assert
        await act.Should().ThrowAsync<RpcException>()
            .Where(e => e.StatusCode == StatusCode.NotFound);
    }

    #endregion

    #region DeleteProduct Tests

    [Fact]
    public async Task DeleteProduct_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        _mockRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        var request = new DeleteProductRequest { Id = 1 };
        var context = TestServerCallContext.Create();

        // Act
        var result = await _service.DeleteProduct(request, context);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("deleted successfully");
    }

    [Fact]
    public async Task DeleteProduct_WithInvalidId_ShouldReturnFailure()
    {
        // Arrange
        _mockRepository.Setup(r => r.DeleteAsync(999)).ReturnsAsync(false);

        var request = new DeleteProductRequest { Id = 999 };
        var context = TestServerCallContext.Create();

        // Act
        var result = await _service.DeleteProduct(request, context);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    #endregion

    #region GetProductJsonLd Tests

    [Fact]
    public async Task GetProductJsonLd_WithValidId_ShouldReturnJsonLdString()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Test Compliment",
            Description = "A test description",
            Category = "Test",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        var request = new GetProductRequest { Id = 1 };
        var context = TestServerCallContext.Create();

        // Act
        var result = await _service.GetProductJsonLd(request, context);

        // Assert
        result.Should().NotBeNull();
        result.JsonLd.Should().NotBeNullOrEmpty();
        result.JsonLd.Should().Contain("@context");
        result.JsonLd.Should().Contain("https://schema.org");
        result.JsonLd.Should().Contain("Test Compliment");
        result.JsonLd.Should().Contain("0.00");
        result.JsonLd.Should().Contain("USD");
    }

    [Fact]
    public async Task GetProductJsonLd_WithInvalidId_ShouldThrowRpcException()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        var request = new GetProductRequest { Id = 999 };
        var context = TestServerCallContext.Create();

        // Act
        Func<Task> act = async () => await _service.GetProductJsonLd(request, context);

        // Assert
        await act.Should().ThrowAsync<RpcException>()
            .Where(e => e.StatusCode == StatusCode.NotFound);
    }

    #endregion

    /// <summary>
    /// Test helper for creating ServerCallContext
    /// </summary>
    private class TestServerCallContext : ServerCallContext
    {
        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders) => Task.CompletedTask;
        protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options) => null!;
        protected override string MethodCore => "TestMethod";
        protected override string HostCore => "localhost";
        protected override string PeerCore => "127.0.0.1";
        protected override DateTime DeadlineCore => DateTime.MaxValue;
        protected override Metadata RequestHeadersCore => new();
        protected override CancellationToken CancellationTokenCore => CancellationToken.None;
        protected override Metadata ResponseTrailersCore => new();
        protected override Status StatusCore { get; set; }
        protected override WriteOptions? WriteOptionsCore { get; set; }
        protected override AuthContext AuthContextCore => null!;

        public static TestServerCallContext Create() => new();
    }
}
