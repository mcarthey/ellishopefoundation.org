using EllisHope.Services;
using System.Net;

namespace EllisHope.Middleware;

/// <summary>
/// Middleware for handling exceptions globally and logging them to the database
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);

            // Handle 404 errors
            if (context.Response.StatusCode == (int)HttpStatusCode.NotFound && !context.Response.HasStarted)
            {
                await HandleNotFoundAsync(context);
            }
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Log to standard logger
        _logger.LogError(exception, "Unhandled exception occurred for request {Path}", context.Request.Path);

        // Get scoped services
        using var scope = context.RequestServices.CreateScope();
        var dbLogger = scope.ServiceProvider.GetService<IDatabaseLoggerService>();
        var emailService = scope.ServiceProvider.GetService<IEmailService>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        // Generate correlation ID for tracking
        var correlationId = context.TraceIdentifier ?? Guid.NewGuid().ToString("N");

        // Log to database
        if (dbLogger != null)
        {
            try
            {
                await dbLogger.LogExceptionAsync(
                    exception,
                    context.Request.Path,
                    context.Request.Method,
                    context.Request.QueryString.ToString(),
                    context.User?.Identity?.IsAuthenticated == true ? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value : null,
                    context.User?.Identity?.Name,
                    GetClientIpAddress(context),
                    context.Request.Headers.UserAgent.ToString(),
                    correlationId);
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Failed to log exception to database");
            }
        }

        // Send error notification email to admin (only for critical errors in production)
        if (!_environment.IsDevelopment() && emailService != null)
        {
            try
            {
                var adminEmail = configuration["Foundation:Email"] ?? "admin@ellishopefoundation.org";
                var subject = $"[ERROR] Website Error - {exception.GetType().Name}";
                var body = BuildErrorEmailBody(exception, context, correlationId);

                await emailService.SendEmailAsync(adminEmail, subject, body, isHtml: true);
            }
            catch (Exception emailEx)
            {
                _logger.LogError(emailEx, "Failed to send error notification email");
            }
        }

        // Clear the response and set error status
        context.Response.Clear();
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "text/html";

        // Store error info for the error page
        context.Items["ErrorCorrelationId"] = correlationId;
        context.Items["ErrorMessage"] = _environment.IsDevelopment() ? exception.Message : "An unexpected error occurred.";
        context.Items["ShowDetails"] = _environment.IsDevelopment();
        context.Items["Exception"] = _environment.IsDevelopment() ? exception : null;

        // Redirect to error page
        context.Response.Redirect($"/Error/500?correlationId={correlationId}");
    }

    private async Task HandleNotFoundAsync(HttpContext context)
    {
        var correlationId = context.TraceIdentifier ?? Guid.NewGuid().ToString("N");

        // Log 404 errors
        using var scope = context.RequestServices.CreateScope();
        var dbLogger = scope.ServiceProvider.GetService<IDatabaseLoggerService>();

        if (dbLogger != null)
        {
            try
            {
                await dbLogger.LogAsync(
                    Models.Domain.AppLogLevel.Warning,
                    "NotFound",
                    $"404 - Page not found: {context.Request.Path}",
                    correlationId: correlationId);
            }
            catch
            {
                // Ignore logging failures for 404s
            }
        }

        context.Response.Redirect($"/Error/404?correlationId={correlationId}");
    }

    private string GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded IP (behind proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',').First().Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private string BuildErrorEmailBody(Exception exception, HttpContext context, string correlationId)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .header {{ background-color: #dc3545; color: white; padding: 15px; border-radius: 5px; }}
        .section {{ margin: 20px 0; padding: 15px; background-color: #f8f9fa; border-radius: 5px; }}
        .label {{ font-weight: bold; color: #495057; }}
        pre {{ background-color: #e9ecef; padding: 10px; border-radius: 3px; overflow-x: auto; font-size: 12px; }}
        .important {{ color: #dc3545; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='header'>
        <h2>Website Error Notification</h2>
    </div>

    <div class='section'>
        <p class='label'>Correlation ID:</p>
        <p class='important'>{correlationId}</p>
        <p><small>Use this ID to find the full log entry in the admin panel.</small></p>
    </div>

    <div class='section'>
        <p class='label'>Error Type:</p>
        <p>{exception.GetType().FullName}</p>
    </div>

    <div class='section'>
        <p class='label'>Error Message:</p>
        <p>{System.Web.HttpUtility.HtmlEncode(exception.Message)}</p>
    </div>

    <div class='section'>
        <p class='label'>Request Details:</p>
        <ul>
            <li><strong>Path:</strong> {context.Request.Path}</li>
            <li><strong>Method:</strong> {context.Request.Method}</li>
            <li><strong>Query:</strong> {context.Request.QueryString}</li>
            <li><strong>User:</strong> {context.User?.Identity?.Name ?? "Anonymous"}</li>
            <li><strong>IP:</strong> {GetClientIpAddress(context)}</li>
            <li><strong>User Agent:</strong> {context.Request.Headers.UserAgent}</li>
        </ul>
    </div>

    <div class='section'>
        <p class='label'>Time:</p>
        <p>{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
    </div>

    <div class='section'>
        <p class='label'>Stack Trace:</p>
        <pre>{System.Web.HttpUtility.HtmlEncode(exception.StackTrace)}</pre>
    </div>

    {(exception.InnerException != null ? $@"
    <div class='section'>
        <p class='label'>Inner Exception:</p>
        <p>{System.Web.HttpUtility.HtmlEncode(exception.InnerException.Message)}</p>
        <pre>{System.Web.HttpUtility.HtmlEncode(exception.InnerException.StackTrace)}</pre>
    </div>
    " : "")}

    <p><a href='https://ellishopefoundation.org/Admin/Logs'>View All Logs in Admin Panel</a></p>
</body>
</html>";
    }
}

/// <summary>
/// Extension methods for adding the error handling middleware
/// </summary>
public static class ErrorHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ErrorHandlingMiddleware>();
    }
}
