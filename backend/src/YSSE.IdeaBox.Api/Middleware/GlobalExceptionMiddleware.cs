using System.Text.Json;
using YSSE.IdeaBox.Api.Common;
using YSSE.IdeaBox.Application.Common;

namespace YSSE.IdeaBox.Api.Middleware;

/// <summary>
/// Converts unhandled exceptions into the standard error envelope. Known
/// <see cref="AppException"/>s keep their status/message; everything else becomes a
/// safe 500. No stack traces, SQL fragments, or internal paths are ever exposed.
/// </summary>
public sealed class GlobalExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationAppException ex)
        {
            _logger.LogWarning(ex, "Validation failure on {Path} ({CorrelationId})", context.Request.Path, context.TraceIdentifier);
            await WriteAsync(context, StatusCodes.Status400BadRequest, "VALIDATION", ex.Message, ex.Errors);
        }
        catch (AppException ex)
        {
            _logger.LogWarning(ex, "Handled application error on {Path} ({CorrelationId})", context.Request.Path, context.TraceIdentifier);
            await WriteAsync(context, ex.StatusCode, CodeFor(ex), ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception on {Path} ({CorrelationId})", context.Request.Path, context.TraceIdentifier);
            await WriteAsync(context, StatusCodes.Status500InternalServerError, "INTERNAL_ERROR",
                "An unexpected error occurred. Please try again.");
        }
    }

    private static string CodeFor(AppException ex) => ex switch
    {
        NotFoundException => "NOT_FOUND",
        ForbiddenException => "FORBIDDEN",
        UnauthorizedException => "UNAUTHORIZED",
        ConflictException => "CONFLICT",
        ValidationAppException => "VALIDATION",
        _ => "REQUEST_FAILED"
    };

    private static async Task WriteAsync(HttpContext context, int statusCode, string code, string message,
        IReadOnlyDictionary<string, string[]>? details = null)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var payload = new ApiErrorResponse
        {
            Success = false,
            TraceId = context.TraceIdentifier,
            Error = new ApiError { Message = message, Code = code, Details = details }
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }
}
