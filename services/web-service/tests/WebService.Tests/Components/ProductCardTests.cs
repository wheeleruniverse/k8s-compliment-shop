using Bunit;
using FluentAssertions;
using Moq;
using WebService.Components.Shared;
using WebService.GraphQL;
using Xunit;

namespace WebService.Tests.Components;

public class ProductCardTests : TestContext
{
    private readonly Mock<IGetProducts_Products_Items> _mockProduct;

    public ProductCardTests()
    {
        _mockProduct = new Mock<IGetProducts_Products_Items>();
        _mockProduct.Setup(p => p.Id).Returns(1);
        _mockProduct.Setup(p => p.Name).Returns("Test Product");
        _mockProduct.Setup(p => p.Description).Returns("Test description");
        _mockProduct.Setup(p => p.Category).Returns("TestCategory");
    }

    [Fact]
    public void ProductCard_Renders_WithProductInformation()
    {
        // Act
        var cut = RenderComponent<ProductCard>(parameters => parameters
            .Add(p => p.Product, _mockProduct.Object));

        // Assert
        cut.Find("h3.product-name").TextContent.Should().Be("Test Product");
        cut.Find("p.product-description").TextContent.Should().Be("Test description");
    }

    [Fact]
    public void ProductCard_DisplaysCategoryBadge()
    {
        // Act
        var cut = RenderComponent<ProductCard>(parameters => parameters
            .Add(p => p.Product, _mockProduct.Object));

        // Assert
        var badge = cut.Find("span.badge");
        badge.Should().NotBeNull();
        badge.TextContent.Should().Be("TestCategory");
        badge.ClassList.Should().Contain("badge-primary");
    }

    [Fact]
    public void ProductCard_HasGlassCardStyling()
    {
        // Act
        var cut = RenderComponent<ProductCard>(parameters => parameters
            .Add(p => p.Product, _mockProduct.Object));

        // Assert
        var card = cut.Find("div.glass-card");
        card.Should().NotBeNull();
        card.ClassList.Should().Contain("product-card");
        card.ClassList.Should().Contain("slide-up");
    }

    [Fact]
    public void ProductCard_ClickInvokesCallback()
    {
        // Arrange
        IGetProducts_Products_Items? clickedProduct = null;
        var cut = RenderComponent<ProductCard>(parameters => parameters
            .Add(p => p.Product, _mockProduct.Object)
            .Add(p => p.OnProductClick, product => { clickedProduct = product; }));

        // Act
        cut.Find("div.glass-card").Click();

        // Assert
        clickedProduct.Should().NotBeNull();
        clickedProduct.Should().Be(_mockProduct.Object);
    }

    [Fact]
    public void ProductCard_ContainsFooter_WithViewDetailsText()
    {
        // Act
        var cut = RenderComponent<ProductCard>(parameters => parameters
            .Add(p => p.Product, _mockProduct.Object));

        // Assert
        var footer = cut.Find("div.product-footer");
        footer.Should().NotBeNull();
        footer.TextContent.Should().Contain("Click to view details");
    }

    [Fact]
    public void ProductCard_RendersAllStructuralElements()
    {
        // Act
        var cut = RenderComponent<ProductCard>(parameters => parameters
            .Add(p => p.Product, _mockProduct.Object));

        // Assert
        cut.Find("div.product-category").Should().NotBeNull();
        cut.Find("h3.product-name").Should().NotBeNull();
        cut.Find("p.product-description").Should().NotBeNull();
        cut.Find("div.product-footer").Should().NotBeNull();
    }

    [Fact]
    public void ProductCard_WithLongDescription_DisplaysFullText()
    {
        // Arrange
        var longDescription = new string('A', 500);
        _mockProduct.Setup(p => p.Description).Returns(longDescription);

        // Act
        var cut = RenderComponent<ProductCard>(parameters => parameters
            .Add(p => p.Product, _mockProduct.Object));

        // Assert
        cut.Find("p.product-description").TextContent.Should().Be(longDescription);
    }

    [Theory]
    [InlineData("Appearance")]
    [InlineData("Professional")]
    [InlineData("Personal")]
    public void ProductCard_WithDifferentCategories_DisplaysCorrectly(string category)
    {
        // Arrange
        _mockProduct.Setup(p => p.Category).Returns(category);

        // Act
        var cut = RenderComponent<ProductCard>(parameters => parameters
            .Add(p => p.Product, _mockProduct.Object));

        // Assert
        cut.Find("span.badge").TextContent.Should().Be(category);
    }
}
