using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace DrugsApp.Infrastructure.Extensions.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(
        RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception exception)
        {
            await WriteInternalServerErrorAsync(httpContext);
        }
    }

    private static async Task WriteInternalServerErrorAsync(HttpContext httpContext)
    {
        if (httpContext.Response.HasStarted)
        {
            throw new InvalidOperationException("The response has already started.");
        }

        var result = new ObjectResult(new
            {
                success = false,
                errorType = "INTERNAL_SERVER_ERROR",
                message = "An unexpected error occurred.",
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                path = httpContext.Request.Path.ToString(),
                traceId = httpContext.TraceIdentifier
            })
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };

        var actionContext = new ActionContext(
            httpContext,
            httpContext.GetRouteData() ?? new RouteData(),
            new ActionDescriptor());

        await result.ExecuteResultAsync(actionContext);
    }
}

