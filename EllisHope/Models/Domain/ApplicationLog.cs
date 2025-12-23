namespace EllisHope.Models.Domain;

/// <summary>
/// Represents a log entry stored in the database for tracking errors, warnings, and informational messages.
/// </summary>
public class ApplicationLog
{
    public int Id { get; set; }

    /// <summary>
    /// The severity level of the log entry
    /// </summary>
    public AppLogLevel Level { get; set; }

    /// <summary>
    /// The category or source of the log (typically the class name)
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// The main log message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Exception details if this log is for an error
    /// </summary>
    public string? ExceptionDetails { get; set; }

    /// <summary>
    /// The stack trace if available
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// The exception type name (e.g., "NullReferenceException")
    /// </summary>
    public string? ExceptionType { get; set; }

    /// <summary>
    /// The inner exception details if available
    /// </summary>
    public string? InnerException { get; set; }

    /// <summary>
    /// The HTTP request path that generated this log
    /// </summary>
    public string? RequestPath { get; set; }

    /// <summary>
    /// The HTTP method (GET, POST, etc.)
    /// </summary>
    public string? HttpMethod { get; set; }

    /// <summary>
    /// The query string if any
    /// </summary>
    public string? QueryString { get; set; }

    /// <summary>
    /// The user ID if the user was authenticated
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// The username if the user was authenticated
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// The client IP address
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// The user agent string
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// A unique correlation ID for tracking related requests
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Additional contextual data in JSON format
    /// </summary>
    public string? AdditionalData { get; set; }

    /// <summary>
    /// The machine name where the log was generated
    /// </summary>
    public string? MachineName { get; set; }

    /// <summary>
    /// When the log entry was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether this log has been reviewed by an admin
    /// </summary>
    public bool IsReviewed { get; set; }

    /// <summary>
    /// When this log was reviewed
    /// </summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>
    /// ID of the user who reviewed this log
    /// </summary>
    public string? ReviewedById { get; set; }

    /// <summary>
    /// Notes added during review
    /// </summary>
    public string? ReviewNotes { get; set; }
}

/// <summary>
/// Log severity levels for application logs
/// </summary>
public enum AppLogLevel
{
    Trace = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
    Critical = 5
}
