using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using Moq;
using WebService.Services;
using Xunit;

namespace WebService.Tests.Unit;

public class AnalyticsServiceTests
{
    private readonly Mock<IJSRuntime> _mockJsRuntime;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly AnalyticsService _analyticsService;

    public AnalyticsServiceTests()
    {
        _mockJsRuntime = new Mock<IJSRuntime>();
        _mockConfiguration = new Mock<IConfiguration>();
        _analyticsService = new AnalyticsService(_mockJsRuntime.Object, _mockConfiguration.Object);
    }

    [Fact]
    public async Task InitializeAsync_WithValidMeasurementId_CallsJavaScriptInitialize()
    {
        // Arrange
        var measurementId = "G-TEST123456";
        _mockConfiguration.Setup(c => c["GoogleAnalytics:MeasurementId"]).Returns(measurementId);

        // Act
        await _analyticsService.InitializeAsync();

        // Assert
        _mockJsRuntime.Verify(
            js => js.InvokeAsync<object>(
                "analyticsHelper.initialize",
                It.Is<object[]>(args => args.Length == 1 && args[0].ToString() == measurementId)),
            Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_WithNullMeasurementId_DoesNotCallJavaScript()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["GoogleAnalytics:MeasurementId"]).Returns((string?)null);

        // Act
        await _analyticsService.InitializeAsync();

        // Assert
        _mockJsRuntime.Verify(
            js => js.InvokeAsync<object>(
                It.IsAny<string>(),
                It.IsAny<object[]>()),
            Times.Never);
    }

    [Fact]
    public async Task InitializeAsync_WithEmptyMeasurementId_DoesNotCallJavaScript()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["GoogleAnalytics:MeasurementId"]).Returns(string.Empty);

        // Act
        await _analyticsService.InitializeAsync();

        // Assert
        _mockJsRuntime.Verify(
            js => js.InvokeAsync<object>(
                It.IsAny<string>(),
                It.IsAny<object[]>()),
            Times.Never);
    }

    [Fact]
    public async Task TrackPageViewAsync_CallsJavaScriptWithCorrectParameters()
    {
        // Arrange
        var measurementId = "G-TEST123456";
        _mockConfiguration.Setup(c => c["GoogleAnalytics:MeasurementId"]).Returns(measurementId);
        var pageTitle = "Home Page";
        var pagePath = "/";

        // Act
        await _analyticsService.TrackPageViewAsync(pageTitle, pagePath);

        // Assert
        _mockJsRuntime.Verify(
            js => js.InvokeAsync<object>(
                "analyticsHelper.trackPageView",
                It.Is<object[]>(args =>
                    args.Length == 2 &&
                    args[0].ToString() == pageTitle &&
                    args[1].ToString() == pagePath)),
            Times.Once);
    }

    [Fact]
    public async Task TrackProductViewAsync_CallsJavaScriptWithCorrectParameters()
    {
        // Arrange
        var measurementId = "G-TEST123456";
        _mockConfiguration.Setup(c => c["GoogleAnalytics:MeasurementId"]).Returns(measurementId);
        var productId = 1;
        var productName = "Test Product";
        var productCategory = "TestCategory";

        // Act
        await _analyticsService.TrackProductViewAsync(productId, productName, productCategory);

        // Assert
        _mockJsRuntime.Verify(
            js => js.InvokeAsync<object>(
                "analyticsHelper.trackProductView",
                It.Is<object[]>(args =>
                    args.Length == 3 &&
                    (int)args[0] == productId &&
                    args[1].ToString() == productName &&
                    args[2].ToString() == productCategory)),
            Times.Once);
    }

    [Fact]
    public async Task TrackEventAsync_WithEventNameOnly_CallsJavaScriptCorrectly()
    {
        // Arrange
        var measurementId = "G-TEST123456";
        _mockConfiguration.Setup(c => c["GoogleAnalytics:MeasurementId"]).Returns(measurementId);
        var eventName = "custom_event";

        // Act
        await _analyticsService.TrackEventAsync(eventName);

        // Assert
        _mockJsRuntime.Verify(
            js => js.InvokeAsync<object>(
                "analyticsHelper.trackEvent",
                It.Is<object[]>(args =>
                    args.Length == 2 &&
                    args[0].ToString() == eventName)),
            Times.Once);
    }

    [Fact]
    public async Task TrackEventAsync_WithEventParams_CallsJavaScriptWithParameters()
    {
        // Arrange
        var measurementId = "G-TEST123456";
        _mockConfiguration.Setup(c => c["GoogleAnalytics:MeasurementId"]).Returns(measurementId);
        var eventName = "custom_event";
        var eventParams = new { param1 = "value1", param2 = 42 };

        // Act
        await _analyticsService.TrackEventAsync(eventName, eventParams);

        // Assert
        _mockJsRuntime.Verify(
            js => js.InvokeAsync<object>(
                "analyticsHelper.trackEvent",
                It.Is<object[]>(args =>
                    args.Length == 2 &&
                    args[0].ToString() == eventName &&
                    args[1] == eventParams)),
            Times.Once);
    }

    [Fact]
    public async Task TrackPageViewAsync_WhenNotInitialized_InitializesFirst()
    {
        // Arrange
        var measurementId = "G-TEST123456";
        _mockConfiguration.Setup(c => c["GoogleAnalytics:MeasurementId"]).Returns(measurementId);
        var pageTitle = "Home Page";
        var pagePath = "/";

        // Act
        await _analyticsService.TrackPageViewAsync(pageTitle, pagePath);

        // Assert - Verify both initialize and trackPageView were called
        _mockJsRuntime.Verify(
            js => js.InvokeAsync<object>(
                "analyticsHelper.initialize",
                It.IsAny<object[]>()),
            Times.Once);

        _mockJsRuntime.Verify(
            js => js.InvokeAsync<object>(
                "analyticsHelper.trackPageView",
                It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_CalledMultipleTimes_OnlyInitializesOnce()
    {
        // Arrange
        var measurementId = "G-TEST123456";
        _mockConfiguration.Setup(c => c["GoogleAnalytics:MeasurementId"]).Returns(measurementId);

        // Act
        await _analyticsService.InitializeAsync();
        await _analyticsService.InitializeAsync();
        await _analyticsService.InitializeAsync();

        // Assert - Should only initialize once
        _mockJsRuntime.Verify(
            js => js.InvokeAsync<object>(
                "analyticsHelper.initialize",
                It.IsAny<object[]>()),
            Times.Once);
    }
}
