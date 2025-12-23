namespace EllisHope.Models.ViewModels;

/// <summary>
/// View model for error pages
/// </summary>
public class ErrorViewModel
{
    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Error title to display
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Correlation ID for tracking the error in logs
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// The original request path
    /// </summary>
    public string? RequestPath { get; set; }

    /// <summary>
    /// Email address for support
    /// </summary>
    public string SupportEmail { get; set; } = "admin@ellishopefoundation.org";

    /// <summary>
    /// Whether to show detailed error information (development only)
    /// </summary>
    public bool ShowDetails { get; set; }

    /// <summary>
    /// The exception if available (development only)
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// Custom message to display
    /// </summary>
    public string? Message { get; set; }
}
