using FluentAssertions;
using ProductService.Models;
using Xunit;

namespace ProductService.Tests.Unit.Models;

public class ProductTests
{
    private const string TestBaseUrl = "https://example.com";
    [Fact]
    public void ToJsonLd_ShouldReturnValidSchemaOrgProduct()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Test Compliment",
            Description = "A test description",
            Category = "Test Category"
        };

        // Act
        var result = product.ToJsonLd();

        // Assert
        result.Should().NotBeNull();
        result.Context.Should().Be("https://schema.org");
        result.Type.Should().Be("Product");
        result.ProductId.Should().Be("1");
        result.Name.Should().Be("Test Compliment");
        result.Description.Should().Be("A test description");
        result.Category.Should().Be("Test Category");
    }

    [Fact]
    public void ToJsonLd_ShouldHaveValidOffer_WithHardcodedPriceAndCurrency()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Test Compliment",
            Description = "A test description",
            Category = "Test Category"
        };

        // Act
        var result = product.ToJsonLd();

        // Assert
        result.Offers.Should().NotBeNull();
        result.Offers.Type.Should().Be("Offer");
        result.Offers.Price.Should().Be("0.00");
        result.Offers.PriceCurrency.Should().Be("USD");
        result.Offers.Availability.Should().Be("https://schema.org/InStock");
    }

    [Fact]
    public void ToJsonLd_WithBaseUrl_ShouldIncludeProductUrl()
    {
        // Arrange
        var product = new Product
        {
            Id = 42,
            Name = "Test Compliment",
            Description = "A test description",
            Category = "Test Category"
        };

        // Act
        var result = product.ToJsonLd(TestBaseUrl);

        // Assert
        result.Url.Should().Be($"{TestBaseUrl}/products/42");
    }

    [Fact]
    public void ToJsonLd_WithoutBaseUrl_ShouldHaveNullUrl()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Test Compliment",
            Description = "A test description",
            Category = "Test Category"
        };

        // Act
        var result = product.ToJsonLd();

        // Assert
        result.Url.Should().BeNull();
    }

    [Fact]
    public void ToJsonLd_WithEmptyBaseUrl_ShouldHaveNullUrl()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Test Compliment",
            Description = "A test description",
            Category = "Test Category"
        };

        // Act
        var result = product.ToJsonLd("");

        // Assert
        result.Url.Should().BeNull();
    }

    [Fact]
    public void Product_DefaultValues_ShouldBeCorrect()
    {
        // Act
        var product = new Product();

        // Assert
        product.Name.Should().Be(string.Empty);
        product.Description.Should().Be(string.Empty);
        product.Category.Should().Be(string.Empty);
        product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        product.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Product_CanSetAllProperties()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var product = new Product
        {
            Id = 5,
            Name = "Amazing Compliment",
            Description = "You are absolutely wonderful",
            Category = "Personal",
            CreatedAt = now,
            UpdatedAt = now
        };

        // Assert
        product.Id.Should().Be(5);
        product.Name.Should().Be("Amazing Compliment");
        product.Description.Should().Be("You are absolutely wonderful");
        product.Category.Should().Be("Personal");
        product.CreatedAt.Should().Be(now);
        product.UpdatedAt.Should().Be(now);
    }
}
