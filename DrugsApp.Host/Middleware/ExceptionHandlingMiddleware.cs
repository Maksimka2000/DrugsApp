using System;
using System.Threading.Tasks;
using DrugsApp.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;

namespace DrugsApp.Infrastructure.Extensions.Middleware;

/// <summary>
/// Converts unhandled exceptions into the shared error envelope.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(
        RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Executes the next middleware and catches unhandled exceptions.
    /// </summary>
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception)
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

        ObjectResult result = ErrorOrExtensions.MapToProblemMiddleware(httpContext);

        var actionContext = new ActionContext(
            httpContext,
            httpContext.GetRouteData() ?? new RouteData(),
            new ActionDescriptor());

        await result.ExecuteResultAsync(actionContext);
    }
}

