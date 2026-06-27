using System.Text.Json;
using System.Text.Json.Serialization;

namespace DrugsApp.IntegrationTests.Common;

internal static class IntegrationTestJson
{
    public static JsonSerializerOptions Options { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
}
