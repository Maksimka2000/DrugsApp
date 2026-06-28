using System.Text.Json;
using System.Text.Json.Serialization;

namespace DrugsApp.IntegrationTests.Common;

internal static class IntegrationTestJson
{
    /// <summary>
    /// Matches ASP.NET Core default request/response JSON (camelCase property names, numeric enums).
    /// </summary>
    public static JsonSerializerOptions Options { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static JsonSerializerOptions ApiRequestOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}
