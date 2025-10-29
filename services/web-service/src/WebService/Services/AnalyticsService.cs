using Microsoft.JSInterop;

namespace WebService.Services;

/// <summary>
/// Service for tracking analytics events with Google Analytics 4
/// </summary>
public class AnalyticsService(IJSRuntime jsRuntime, IConfiguration configuration)
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;
    private readonly IConfiguration _configuration = configuration;
    private bool _initialized;

    /// <summary>
    /// Initialize Google Analytics with the configured measurement ID
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        var measurementId = _configuration["GoogleAnalytics:MeasurementId"];

        if (string.IsNullOrEmpty(measurementId))
        {
            Console.WriteLine("Google Analytics measurement ID not configured");
            return;
        }

        try
        {
            await _jsRuntime.InvokeVoidAsync("analyticsHelper.initialize", measurementId);
            _initialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize analytics: {ex.Message}");
        }
    }

    /// <summary>
    /// Track a page view event
    /// </summary>
    public async Task TrackPageViewAsync(string pageTitle, string pagePath)
    {
        if (!_initialized)
            await InitializeAsync();

        try
        {
            await _jsRuntime.InvokeVoidAsync("analyticsHelper.trackPageView", pageTitle, pagePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to track page view: {ex.Message}");
        }
    }

    /// <summary>
    /// Track a product view event
    /// </summary>
    public async Task TrackProductViewAsync(int productId, string productName, string productCategory)
    {
        if (!_initialized)
            await InitializeAsync();

        try
        {
            await _jsRuntime.InvokeVoidAsync("analyticsHelper.trackProductView",
                productId, productName, productCategory);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to track product view: {ex.Message}");
        }
    }

    /// <summary>
    /// Track a custom event
    /// </summary>
    public async Task TrackEventAsync(string eventName, object? eventParams = null)
    {
        if (!_initialized)
            await InitializeAsync();

        try
        {
            await _jsRuntime.InvokeVoidAsync("analyticsHelper.trackEvent", eventName, eventParams);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to track event: {ex.Message}");
        }
    }
}
