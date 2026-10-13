using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.UserRecordLimitsFeature;

public sealed class UserRecordLimitsTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateOtherAllergy_WhenUserHas50_RejectsTheNextOne()
    {
        await AuthenticateFreshClientAsync();

        HttpResponseMessage rejected = await CreateUntilLimitAsync(
            "/api/allergies/other-allergies",
            index => new { allergyName = $"Environmental allergen {index}", note = "note" },
            expectedLimit: 50);

        await AssertLimitRejectionAsync(rejected, expectedLimit: 50);
    }

    [Fact]
    public async Task CreateDrugProblem_WhenUserHas50_RejectsTheNextOneIncludingCustom()
    {
        await AuthenticateFreshClientAsync();

        HttpResponseMessage rejected = await CreateUntilLimitAsync(
            "/api/drug-problems",
            index => new
            {
                drugFormId = (Guid?)null,
                customDrugName = $"Custom problem drug {index}",
                customActiveSubstances = "Substance",
                customFormulationName = "Tablet",
                customDosage = "10 mg",
                problemNote = "note"
            },
            expectedLimit: 50);

        await AssertLimitRejectionAsync(rejected, expectedLimit: 50);
    }

    [Fact]
    public async Task CreateMealTime_WhenUserHas20_RejectsTheNextOne()
    {
        await AuthenticateFreshClientAsync();

        HttpResponseMessage rejected = await CreateUntilLimitAsync(
            "/api/meal-times",
            index => new
            {
                mealName = $"Meal {index}",
                intakeTime = $"{index % 24:00}:{index / 24 * 15:00}:00"
            },
            expectedLimit: 20);

        await AssertLimitRejectionAsync(rejected, expectedLimit: 20);
    }

    private async Task AuthenticateFreshClientAsync()
    {
        string email = IntegrationAuthFlows.CreateUniqueEmail();
        const string password = "LimitsPass123!";

        await IntegrationAuthFlows.RegisterAndConfirmMemberAsync(
            Client,
            EmailSender,
            email,
            password,
            CancellationToken.None);

        var login = await TestAuthHelper.LoginMemberAsync(Client, email, password, CancellationToken.None);
        TestAuthHelper.ApplyBearerToken(Client, login.AccessToken);
    }

    private async Task<HttpResponseMessage> CreateUntilLimitAsync(
        string url,
        Func<int, object> bodyFactory,
        int expectedLimit)
    {
        for (int index = 0; index < expectedLimit; index++)
        {
            HttpResponseMessage created = await Client.PostAsJsonAsync(
                url,
                bodyFactory(index),
                IntegrationTestJson.ApiRequestOptions,
                CancellationToken.None);

            string body = await created.Content.ReadAsStringAsync(CancellationToken.None);
            created.IsSuccessStatusCode.Should().BeTrue(because: $"record {index + 1} is within the cap: {body}");
        }

        return await Client.PostAsJsonAsync(
            url,
            bodyFactory(expectedLimit),
            IntegrationTestJson.ApiRequestOptions,
            CancellationToken.None);
    }

    private static async Task AssertLimitRejectionAsync(HttpResponseMessage response, int expectedLimit)
    {
        string body = await response.Content.ReadAsStringAsync(CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict, because: body);

        using JsonDocument document = JsonDocument.Parse(body);
        Descendants(document.RootElement)
            .Where(element => element.ValueKind == JsonValueKind.Object
                && element.TryGetProperty("maxCount", out JsonElement max)
                && max.ValueKind == JsonValueKind.Number
                && max.GetInt32() == expectedLimit)
            .Should().NotBeEmpty(because: body);
    }

    private static IEnumerable<JsonElement> Descendants(JsonElement element)
    {
        yield return element;

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty property in element.EnumerateObject())
            {
                foreach (JsonElement child in Descendants(property.Value))
                {
                    yield return child;
                }
            }
        }
    }
}
