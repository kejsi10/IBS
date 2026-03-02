using System.Net;
using System.Text.Json;
using IBS.BuildingBlocks.Domain;
using Microsoft.EntityFrameworkCore;

namespace IBS.Api.Middleware;

/// <summary>
/// Middleware for handling exceptions globally.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the ExceptionHandlingMiddleware class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger.</param>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, errors) = exception switch
        {
            BusinessRuleViolationException businessEx =>
                (HttpStatusCode.BadRequest, businessEx.Message, (string[]?)null),
            EntityNotFoundException notFoundEx =>
                (HttpStatusCode.NotFound, notFoundEx.Message, null),
            DbUpdateConcurrencyException =>
                (HttpStatusCode.Conflict,
                 "The resource was modified by another user. Please reload and try again.", null),
            ConcurrencyConflictException =>
                (HttpStatusCode.PreconditionFailed,
                 "The resource has been modified since it was last retrieved.", null),
            UnauthorizedAccessException =>
                (HttpStatusCode.Unauthorized, "Unauthorized access.", null),
            ArgumentException argEx =>
                (HttpStatusCode.BadRequest, argEx.Message, null),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", null)
        };

        // Set ETag header on concurrency conflict so the client can retry with the correct value
        if (exception is ConcurrencyConflictException concurrencyEx)
        {
            context.Response.Headers.ETag = $"\"{concurrencyEx.CurrentETag}\"";
        }

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning(exception, "A handled exception occurred: {Message}", exception.Message);
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            Status = (int)statusCode,
            Message = message,
            Errors = errors,
            TraceId = context.TraceIdentifier
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

/// <summary>
/// Error response model.
/// </summary>
public sealed class ErrorResponse
{
    /// <summary>
    /// Gets or sets the HTTP status code.
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional error details.
    /// </summary>
    public string[]? Errors { get; set; }

    /// <summary>
    /// Gets or sets the trace identifier for debugging.
    /// </summary>
    public string? TraceId { get; set; }
}

/// <summary>
/// Extension methods for ExceptionHandlingMiddleware.
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    /// <summary>
    /// Adds exception handling middleware to the pipeline.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
