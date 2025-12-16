using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace DrugsApp.Host.Exceptions;

/// <summary>
/// Handles unexpected exceptions and emits the shared problem response.
/// </summary>
internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Logs the exception and writes the standard problem details payload.
    /// </summary>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (httpContext.Response.HasStarted)
        {
            return false;
        }

        _logger.LogError(exception, "An unhandled exception occurred while processing the request.");

        ObjectResult problem = ErrorOrExtensions.MapToGlobalProblem(httpContext);

        var actionContext = new ActionContext(
            httpContext,
            httpContext.GetRouteData() ?? new RouteData(),
            new ActionDescriptor());

        await problem.ExecuteResultAsync(actionContext);

        return true;
    }
}

