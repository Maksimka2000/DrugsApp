using Serilog.Context;

namespace DrugsApp.Infrastructure.Extensions.Middleware;

/// <summary>
/// Pushes the current trace identifier into Serilog's log context.
/// </summary>
public class RequestLogContextMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestLogContextMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware delegate in the pipeline.</param>
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
