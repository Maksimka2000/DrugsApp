using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace DrugsApp.Features;

public static class ErrorOrExtensions
{
    public static ActionResult MapToProblem(this List<Error> errors, HttpContext httpContext)
    {
        if (errors.Count == 0)
        {
            return new ObjectResult(new
            {
                success = false,
                errorType = "INTERNAL_SERVER_ERROR",
                message = "An unexpected error occurred with ErrorOrExtensions.",
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                path = httpContext.Request.Path.ToString(),
                traceId = httpContext.TraceIdentifier
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        Error firstError = errors[0];

        return firstError.Type switch
        {
            ErrorType.Validation => MapValidationError(firstError, httpContext),
            ErrorType.NotFound => MapNotFoundError(firstError, httpContext),
            ErrorType.Conflict => MapConflictError(firstError, httpContext),
            _ => MapInternalError(firstError, httpContext)
        };
    }

    private static ActionResult MapValidationError(Error error, HttpContext httpContext)
    {
        var response = new Dictionary<string, object>
        {
            ["success"] = false,
            ["errorType"] = "REQUEST_VALIDATION_ERROR",
            ["message"] = error.Description,
            ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            ["path"] = httpContext.Request.Path.ToString(),
            ["traceId"] = httpContext.TraceIdentifier
        };

        // Add metadata fields to response
        if (error.Metadata != null)
        {
            foreach (var (key, value) in error.Metadata)
            {
                response[key] = value;
            }
        }

        return new ObjectResult(response)
        {
            StatusCode = StatusCodes.Status400BadRequest
        };
    }

    private static ActionResult MapNotFoundError(Error error, HttpContext httpContext)
    {
        var response = new Dictionary<string, object>
        {
            ["success"] = false,
            ["errorType"] = "NOT_FOUND",
            ["message"] = error.Description,
            ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            ["path"] = httpContext.Request.Path.ToString(),
            ["traceId"] = httpContext.TraceIdentifier
        };

        // Add metadata fields to response
        if (error.Metadata != null)
        {
            foreach (var (key, value) in error.Metadata)
            {
                response[key] = value;
            }
        }

        return new ObjectResult(response)
        {
            StatusCode = StatusCodes.Status404NotFound
        };
    }

    private static ActionResult MapConflictError(Error error, HttpContext httpContext)
    {
        var response = new Dictionary<string, object>
        {
            ["success"] = false,
            ["errorType"] = "CONFLICT",
            ["message"] = error.Description,
            ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            ["path"] = httpContext.Request.Path.ToString(),
            ["traceId"] = httpContext.TraceIdentifier
        };

        // Add metadata fields to response
        if (error.Metadata != null)
        {
            foreach (var (key, value) in error.Metadata)
            {
                response[key] = value;
            }
        }

        return new ObjectResult(response)
        {
            StatusCode = StatusCodes.Status409Conflict
        };
    }

    private static ActionResult MapInternalError(Error error, HttpContext httpContext)
    {
        var response = new Dictionary<string, object>
        {
            ["success"] = false,
            ["errorType"] = "INTERNAL_SERVER_ERROR",
            ["message"] = error.Description,
            ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            ["path"] = httpContext.Request.Path.ToString(),
            ["traceId"] = httpContext.TraceIdentifier
        };

        // Add metadata fields to response
        if (error.Metadata != null)
        {
            foreach (var (key, value) in error.Metadata)
            {
                response[key] = value;
            }
        }

        return new ObjectResult(response)
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
    }
}
