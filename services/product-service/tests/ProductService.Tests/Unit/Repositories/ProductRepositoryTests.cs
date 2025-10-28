using FluentAssertions;
using ProductService.Models;
using ProductService.Repositories;
using ProductService.Tests.Helpers;
using Xunit;

namespace ProductService.Tests.Unit.Repositories;

public class ProductRepositoryTests : IDisposable
{
    private readonly ProductRepository _repository;
    private readonly Data.ProductDbContext _context;

    public ProductRepositoryTests()
    {
        _context = DbContextFactory.CreateInMemoryContextWithSeedData();
        _repository = new ProductRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnProduct()
    {
        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Excellent Haircut Compliment");
        result.Category.Should().Be("Appearance");
    }

    [Theory]
    [InlineData(999)]  // Non-existent ID
    [InlineData(0)]    // Zero ID
    [InlineData(-1)]   // Negative ID
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull(int invalidId)
    {
        // Act
        var result = await _repository.GetByIdAsync(invalidId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithoutFilter_ShouldReturnAllProducts()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(6);
        result.Should().BeInAscendingOrder(p => p.Category).And.ThenBeInAscendingOrder(p => p.Name);
    }

    [Fact]
    public async Task GetAllAsync_WithCategoryFilter_ShouldReturnFilteredProducts()
    {
        // Act
        var result = await _repository.GetAllAsync(category: "Appearance");

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(p => p.Category.Should().Be("Appearance"));
    }

    [Fact]
    public async Task GetAllAsync_WithNonExistentCategory_ShouldReturnEmpty()
    {
        // Act
        var result = await _repository.GetAllAsync(category: "NonExistent");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Act
        var result = await _repository.GetAllAsync(page: 1, pageSize: 2);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_WithSecondPage_ShouldReturnDifferentResults()
    {
        // Arrange
        var page1 = await _repository.GetAllAsync(page: 1, pageSize: 2);

        // Act
        var page2 = await _repository.GetAllAsync(page: 2, pageSize: 2);

        // Assert
        page2.Should().HaveCount(2);
        page2.Select(p => p.Id).Should().NotIntersectWith(page1.Select(p => p.Id));
    }

    [Fact]
    public async Task GetAllAsync_WithPageSizeLargerThanResults_ShouldReturnAllResults()
    {
        // Act
        var result = await _repository.GetAllAsync(page: 1, pageSize: 100);

        // Assert
        result.Should().HaveCount(6);
    }

    [Fact]
    public async Task GetAllAsync_WithCategoryAndPagination_ShouldApplyBothFilters()
    {
        // Act
        var result = await _repository.GetAllAsync(category: "Personal", page: 1, pageSize: 1);

        // Assert
        result.Should().HaveCount(1);
        result.First().Category.Should().Be("Personal");
    }

    #endregion

    #region GetTotalCountAsync Tests

    [Fact]
    public async Task GetTotalCountAsync_WithoutFilter_ShouldReturnTotalCount()
    {
        // Act
        var result = await _repository.GetTotalCountAsync();

        // Assert
        result.Should().Be(6);
    }

    [Fact]
    public async Task GetTotalCountAsync_WithCategoryFilter_ShouldReturnFilteredCount()
    {
        // Act
        var result = await _repository.GetTotalCountAsync(category: "Professional");

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task GetTotalCountAsync_WithNonExistentCategory_ShouldReturnZero()
    {
        // Act
        var result = await _repository.GetTotalCountAsync(category: "NonExistent");

        // Assert
        result.Should().Be(0);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidProduct_ShouldAddToDatabase()
    {
        // Arrange
        var product = new Product
        {
            Name = "New Compliment",
            Description = "A brand new compliment",
            Category = "Test"
        };

        // Act
        var result = await _repository.CreateAsync(product);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("New Compliment");

        var fromDb = await _repository.GetByIdAsync(result.Id);
        fromDb.Should().NotBeNull();
        fromDb!.Name.Should().Be("New Compliment");
    }

    [Fact]
    public async Task CreateAsync_ShouldSetTimestamps()
    {
        // Arrange
        var beforeCreate = DateTime.UtcNow;
        var product = new Product
        {
            Name = "New Compliment",
            Description = "A brand new compliment",
            Category = "Test"
        };

        // Act
        var result = await _repository.CreateAsync(product);
        var afterCreate = DateTime.UtcNow;

        // Assert
        result.CreatedAt.Should().BeOnOrAfter(beforeCreate).And.BeOnOrBefore(afterCreate);
        result.UpdatedAt.Should().BeOnOrAfter(beforeCreate).And.BeOnOrBefore(afterCreate);
    }

    [Fact]
    public async Task CreateAsync_ShouldIncrementTotalCount()
    {
        // Arrange
        var initialCount = await _repository.GetTotalCountAsync();
        var product = new Product
        {
            Name = "New Compliment",
            Description = "A brand new compliment",
            Category = "Test"
        };

        // Act
        await _repository.CreateAsync(product);

        // Assert
        var newCount = await _repository.GetTotalCountAsync();
        newCount.Should().Be(initialCount + 1);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidProduct_ShouldUpdateDatabase()
    {
        // Arrange
        var product = await _repository.GetByIdAsync(1);
        product!.Name = "Updated Name";
        product.Description = "Updated Description";

        // Act
        var result = await _repository.UpdateAsync(product);

        // Assert
        result.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Description");

        var fromDb = await _repository.GetByIdAsync(1);
        fromDb!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateTimestamp()
    {
        // Arrange
        var product = await _repository.GetByIdAsync(1);
        var originalUpdatedAt = product!.UpdatedAt;
        await Task.Delay(10); // Small delay to ensure timestamp difference

        product.Name = "Updated Name";

        // Act
        var result = await _repository.UpdateAsync(product);

        // Assert
        result.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_ShouldNotChangeCreatedAt()
    {
        // Arrange
        var product = await _repository.GetByIdAsync(1);
        var originalCreatedAt = product!.CreatedAt;

        product.Name = "Updated Name";

        // Act
        var result = await _repository.UpdateAsync(product);

        // Assert
        result.CreatedAt.Should().Be(originalCreatedAt);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldRemoveFromDatabase()
    {
        // Arrange
        var initialCount = await _repository.GetTotalCountAsync();

        // Act
        var result = await _repository.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();

        var fromDb = await _repository.GetByIdAsync(1);
        fromDb.Should().BeNull();

        var newCount = await _repository.GetTotalCountAsync();
        newCount.Should().Be(initialCount - 1);
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldNotAffectOtherProducts()
    {
        // Arrange
        var product2 = await _repository.GetByIdAsync(2);

        // Act
        await _repository.DeleteAsync(1);

        // Assert
        var stillExists = await _repository.GetByIdAsync(2);
        stillExists.Should().NotBeNull();
        stillExists!.Name.Should().Be(product2!.Name);
    }

    #endregion
}
