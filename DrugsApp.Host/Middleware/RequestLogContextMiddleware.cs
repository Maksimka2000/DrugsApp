using Serilog.Context;

namespace DrugsApp.Infrastructure.Extensions.Middleware;

public class RequestLogContextMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLogContextMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContext httpContext)
    {
        using (LogContext.PushProperty("traceId", httpContext.TraceIdentifier))
        {
            return _next(httpContext);
        }
    }
}
