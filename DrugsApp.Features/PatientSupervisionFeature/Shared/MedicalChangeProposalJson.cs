using System.Text.Json;
using ErrorOr;

namespace DrugsApp.Features.PatientSupervisionFeature.Shared;

internal static class MedicalChangeProposalJson
{
    private static readonly string[] ForbiddenRootPropertyNames =
    [
        "delete",
        "remove",
        "update",
        "operation",
        "patch"
    ];

    internal static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    internal static ErrorOr<T> Deserialize<T>(string payload, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return Error.Validation(
                description: $"{fieldName} is required.",
                metadata: new() { ["field"] = fieldName });
        }

        try
        {
            T? value = JsonSerializer.Deserialize<T>(payload, SerializerOptions);
            if (value is null)
            {
                return Error.Validation(
                    description: $"Invalid {fieldName}.",
                    metadata: new() { ["field"] = fieldName });
            }

            return value;
        }
        catch (JsonException)
        {
            return Error.Validation(
                description: $"Invalid {fieldName} JSON.",
                metadata: new() { ["field"] = fieldName });
        }
    }

    internal static ErrorOr<Success> ValidateAddOnlyPayload(string payload, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return Error.Validation(
                description: $"{fieldName} is required.",
                metadata: new() { ["field"] = fieldName });
        }

        try
        {
            using var document = JsonDocument.Parse(payload);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return Error.Validation(
                    description: $"{fieldName} must be a JSON object.",
                    metadata: new() { ["field"] = fieldName });
            }

            foreach (string forbidden in ForbiddenRootPropertyNames)
            {
                if (document.RootElement.TryGetProperty(forbidden, out _))
                {
                    return Error.Validation(
                        description: $"{fieldName} must describe an add-only operation.",
                        metadata: new() { ["field"] = fieldName });
                }
            }

            return Result.Success;
        }
        catch (JsonException)
        {
            return Error.Validation(
                description: $"Invalid {fieldName} JSON.",
                metadata: new() { ["field"] = fieldName });
        }
    }
}
