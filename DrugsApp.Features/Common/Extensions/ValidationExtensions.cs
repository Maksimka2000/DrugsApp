using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace DrugsApp.Features;


public static class ValidationExtensions
{
    public static ActionResult ToValidationProblem(
        this ValidationResult validationResult,
        HttpContext httpContext)
    {
        var errors = validationResult.Errors
            .GroupBy(e => ToCamelCase(e.PropertyName))
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        var response = new
        {
            success = false,
            errorType = "REQUEST_VALIDATION_ERROR",
            message = "Request validation failed. Please check your input data.",
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            path = httpContext.Request.Path.ToString(),
            traceId = httpContext.TraceIdentifier,
            errors
        };

        return new ObjectResult(response)
        {
            StatusCode = StatusCodes.Status400BadRequest
        };
    }

    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
            return str;

        return char.ToLowerInvariant(str[0]) + str.Substring(1);
    }
}