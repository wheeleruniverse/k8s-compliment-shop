using Bunit;
using FluentAssertions;
using WebService.Components.Shared;
using Xunit;

namespace WebService.Tests.Components;

public class SkeletonLoaderTests : TestContext
{
    [Fact]
    public void SkeletonLoader_Renders_WithCorrectStructure()
    {
        // Act
        var cut = RenderComponent<SkeletonLoader>();

        // Assert - Verify main container and key elements
        var rootDiv = cut.Find("div.glass-card.skeleton-card");
        rootDiv.Should().NotBeNull();

        var skeletonImage = cut.Find("div.skeleton-image");
        skeletonImage.Should().NotBeNull();

        var skeletonTitle = cut.Find("div.skeleton-title");
        skeletonTitle.Should().NotBeNull();

        var textSkeletons = cut.FindAll("div.skeleton-text");
        textSkeletons.Count.Should().Be(3);
    }

    [Fact]
    public void SkeletonLoader_HasGlassCardClass()
    {
        // Act
        var cut = RenderComponent<SkeletonLoader>();

        // Assert
        var rootDiv = cut.Find("div.glass-card");
        rootDiv.Should().NotBeNull();
        rootDiv.ClassList.Should().Contain("skeleton-card");
    }

    [Fact]
    public void SkeletonLoader_ContainsSkeletonImage()
    {
        // Act
        var cut = RenderComponent<SkeletonLoader>();

        // Assert
        var skeletonImage = cut.Find("div.skeleton-image");
        skeletonImage.Should().NotBeNull();
        skeletonImage.ClassList.Should().Contain("skeleton");
    }

    [Fact]
    public void SkeletonLoader_ContainsSkeletonTitle()
    {
        // Act
        var cut = RenderComponent<SkeletonLoader>();

        // Assert
        var skeletonTitle = cut.Find("div.skeleton-title");
        skeletonTitle.Should().NotBeNull();
        skeletonTitle.ClassList.Should().Contain("skeleton");
    }

    [Fact]
    public void SkeletonLoader_ContainsThreeTextSkeletons()
    {
        // Act
        var cut = RenderComponent<SkeletonLoader>();

        // Assert
        var textSkeletons = cut.FindAll("div.skeleton-text");
        textSkeletons.Count.Should().Be(3);
    }

    [Fact]
    public void SkeletonLoader_TextSkeletonsHaveCorrectWidths()
    {
        // Act
        var cut = RenderComponent<SkeletonLoader>();

        // Assert
        var textSkeletons = cut.FindAll("div.skeleton-text");

        // First one has no width style (100% default)
        textSkeletons[0].GetAttribute("style").Should().BeNullOrEmpty();

        // Second one has 80% width
        textSkeletons[1].GetAttribute("style").Should().Contain("width: 80%");

        // Third one has 90% width
        textSkeletons[2].GetAttribute("style").Should().Contain("width: 90%");
    }
}
