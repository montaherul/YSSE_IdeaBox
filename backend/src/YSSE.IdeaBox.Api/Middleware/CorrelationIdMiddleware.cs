using Serilog.Context;

namespace YSSE.IdeaBox.Api.Middleware;

/// <summary>
/// Honors or produces an <c>X-Correlation-ID</c> for every request, aligns it with
/// <see cref="HttpContext.TraceIdentifier"/>, echoes it back, and pushes it onto the
/// Serilog log context so structured logs can be correlated end to end.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var values)
            && !string.IsNullOrWhiteSpace(values.ToString())
                ? values.ToString()
                : Guid.NewGuid().ToString("N");

        context.TraceIdentifier = correlationId;
        context.Response.Headers[HeaderName] = correlationId;
        context.Response.Headers["X-Trace-Id"] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
