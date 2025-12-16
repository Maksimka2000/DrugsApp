using Serilog.Context;

namespace DrugsApp.Infrastructure.Extensions.Middleware;

/// <summary>
/// Pushes the current trace identifier into Serilog's log context.
/// </summary>
public class RequestLogContextMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLogContextMiddleware(RequestDelegate next) => _next = next;

    /// <summary>
    /// Adds the traceId property for the duration of the request pipeline.
    /// </summary>
    public Task InvokeAsync(HttpContext httpContext)
    {
        using (LogContext.PushProperty("traceId", httpContext.TraceIdentifier))
        {
            return _next(httpContext);
        }
    }
}
