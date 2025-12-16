using System.Collections.Generic;
using System.Linq;
using ErrorOr;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features;

/// <summary>
/// Maps ErrorOr results to the standardized HTTP problem envelope.
/// </summary>
public static class ErrorOrExtensions
{
    
    /// <summary>
    /// Converts ErrorOr responses to a problem result with trace metadata.
    /// </summary>
    public static ActionResult MapToProblem(
        this List<Error> errors,
        HttpContext httpContext)
    {

        if (errors.Count == 0)
        {
            return BuildProblemResult(
                httpContext,
                StatusCodes.Status500InternalServerError,
                "UnexpectedError",
                "An unknown error occurred while processing the request.");
        }

        Error firstError = errors[0];

        return firstError.Type switch
        {
            ErrorType.Validation => BuildProblemResult(
                httpContext,
                StatusCodes.Status400BadRequest,
                "ValidationError",
                firstError.Description,
                firstError.Metadata),
            ErrorType.NotFound => BuildProblemResult(
                httpContext,
                StatusCodes.Status404NotFound,
                "NotFoundError",
                firstError.Description,
                firstError.Metadata),
            ErrorType.Conflict => BuildProblemResult(
                httpContext,
                StatusCodes.Status409Conflict,
                "ConflinctError",
                firstError.Description,
                firstError.Metadata),
            _ => BuildProblemResult(
                httpContext,
                StatusCodes.Status500InternalServerError,
                "UnexpectedError",
                "An unknown error occurred while processing the request."),
        };
    }


    /// <summary>
    /// Builds a validation problem response from a FluentValidation result.
    /// </summary>
    public static ActionResult MapToProblem(
        this ValidationResult validationResult,
        HttpContext httpContext)
    {
       
        Dictionary<string, object?>[] errors = validationResult.Errors
        .Select(f => new Dictionary<string, object?>
        {
            ["pointer"] = f.PropertyName,
            ["detail"] = f.ErrorMessage
        })
        .ToArray();

        var metadata = new Dictionary<string, object?>
        {
            ["errors"] = errors
        };

        return BuildProblemResult(
            httpContext,
            StatusCodes.Status400BadRequest,
            "ValidationError",
            "Request payload contains validation errors.",
            metadata);
    }


    public static ObjectResult MapToGlobalProblem(HttpContext httpContext)
    {
        return BuildProblemResult(
                httpContext,
                StatusCodes.Status500InternalServerError,
                "UnexpectedError",
                "An unknown error occurred while processing the request.");
    }



    /// <summary>
    /// Builds a problem response for any caller that needs the shared envelope.
    /// </summary>
    private static ObjectResult BuildProblemResult(
        HttpContext httpContext,
        int status,
        string title,
        string detail,
        IReadOnlyDictionary<string, object?>? extensionMembers = null)
    {
        var envelope = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
        };


        var extensionPayload = new Dictionary<string, object?>
        {
            ["Timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            ["TraceId"] = httpContext.TraceIdentifier
        };

        if (extensionMembers is not null)
        {
            foreach ((string? key, object? value) in extensionMembers)
            {
                extensionPayload[key] = value;
            }
        }

        envelope.Extensions["Extensions"] = extensionPayload;

        return new ObjectResult(envelope)
        {
            StatusCode = status
        };
    }
}
