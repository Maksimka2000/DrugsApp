using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Enums;
using DrugsApp.Infrastructure.Persistence;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DrugsApp.IntegrationTests.DrugInteractionToolFeature;

/// <summary>
/// Guards the per-medication active substance to food and supplement interaction sections of the registered report.
/// </summary>
public sealed class RegisteredDrugInteractionToolReportFoodSupplementTests : IntegrationTestBase
{
    [Fact]
    public async Task Report_WhenAllFoodAndSupplementInteractionsAreRequested_ReturnsThePossibleInteractionsOfEachMedication()
    {
        Guid foodForm = await FindFormWithPartnerAsync(ShortInteractionType.Food);
        Guid supplementForm = await FindFormWithPartnerAsync(ShortInteractionType.Supplement);

        JsonElement report = await GetReportAsync(
            new[] { foodForm, supplementForm }.Distinct().ToArray(),
            showAllFoodAndSupplementInteractions: true);

        JsonElement[] foodAllOther = FindSections(report, "specific_drug.foods.all_other").ToArray();
        JsonElement[] supplementAllOther = FindSections(report, "specific_drug.supplements.all_other").ToArray();

        foodAllOther.Sum(section => section.GetProperty("recordCount").GetInt32())
            .Should().BeGreaterThan(0, because: "the selected medication has a catalog food interaction");
        supplementAllOther.Sum(section => section.GetProperty("recordCount").GetInt32())
            .Should().BeGreaterThan(0, because: "the selected medication has a catalog supplement interaction");

        foreach (JsonElement row in foodAllOther.SelectMany(section => section.GetProperty("rows").EnumerateArray()))
        {
            row.GetProperty("activeSubstanceName").GetString().Should().NotBeNullOrWhiteSpace();
            row.GetProperty("foodName").GetString().Should().NotBeNullOrWhiteSpace();
            row.GetProperty("severity").GetString().Should().NotBeNullOrWhiteSpace();
            row.GetProperty("displayLine").GetString().Should().Contain("=>");
        }

        foreach (JsonElement row in supplementAllOther.SelectMany(section => section.GetProperty("rows").EnumerateArray()))
        {
            row.GetProperty("supplementName").GetString().Should().NotBeNullOrWhiteSpace();
            row.GetProperty("displayLine").GetString().Should().Contain("=>");
        }
    }

    [Fact]
    public async Task Report_WhenOnlyUserDataIsRequested_ReturnsTakenNowSectionsAndNoPossibleInteractions()
    {
        Guid foodForm = await FindFormWithPartnerAsync(ShortInteractionType.Food);

        JsonElement report = await GetReportAsync([foodForm], showAllFoodAndSupplementInteractions: false);

        FindSections(report, "specific_drug.foods.taken_now").Should().NotBeEmpty();
        FindSections(report, "specific_drug.supplements.taken_now").Should().NotBeEmpty();
        FindSections(report, "specific_drug.foods.all_other").Should().BeEmpty();
        FindSections(report, "specific_drug.supplements.all_other").Should().BeEmpty();
    }

    private async Task<Guid> FindFormWithPartnerAsync(ShortInteractionType partnerType)
    {
        using IServiceScope scope = Factory.Services.CreateScope();
        DrugsAppDbContext dbContext = scope.ServiceProvider.GetRequiredService<DrugsAppDbContext>();

        InteractionExpanded row = await dbContext.InteractionExpandedRows
            .AsNoTracking()
            .Where(candidate => candidate.PartyTypeA == ShortInteractionType.Substance
                && candidate.PartyTypeB == partnerType)
            .OrderBy(candidate => candidate.Id)
            .FirstAsync();

        return await dbContext.Set<DrugForm>()
            .AsNoTracking()
            .Where(form => form.Drug.ActiveSubstances.Any(substance => substance.Id == row.PartyIdA)
                && form.Drug.TypeOfDrug == DrugType.Standard
                && form.Drug.TypeOfProduct != "Weterynaryjny")
            .OrderBy(form => form.Id)
            .Select(form => form.Id)
            .FirstAsync();
    }

    private async Task<JsonElement> GetReportAsync(Guid[] manualDrugFormIds, bool showAllFoodAndSupplementInteractions)
    {
        TimeSpan stepTimeout = TimeSpan.FromSeconds(60);

        var login = await TestAuthHelper.LoginAsClientAsync(Client, CancellationToken.None).WaitAsync(stepTimeout);
        TestAuthHelper.ApplyBearerToken(Client, login.AccessToken);

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/tools/drug-interactions/registered/report",
            new
            {
                manualDrugFormIds,
                includeUserData = true,
                showAllDrugsInteractions = false,
                showOralGuidance = false,
                showAllFoodAndSupplementInteractions
            },
            IntegrationTestJson.ApiRequestOptions,
            CancellationToken.None).WaitAsync(stepTimeout);

        string body = await response.Content.ReadAsStringAsync(CancellationToken.None).WaitAsync(stepTimeout);
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);

        return JsonDocument.Parse(body).RootElement.Clone();
    }

    private static IEnumerable<JsonElement> FindSections(JsonElement root, string sectionCode) =>
        Descendants(root)
            .Where(element => element.ValueKind == JsonValueKind.Object
                && element.TryGetProperty("sectionCode", out JsonElement code)
                && code.ValueKind == JsonValueKind.String
                && code.GetString() == sectionCode);

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
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in element.EnumerateArray())
            {
                foreach (JsonElement child in Descendants(item))
                {
                    yield return child;
                }
            }
        }
    }
}
