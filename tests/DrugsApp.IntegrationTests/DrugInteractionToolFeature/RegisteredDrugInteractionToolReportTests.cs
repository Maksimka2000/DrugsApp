using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Infrastructure.Persistence;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DrugsApp.IntegrationTests.DrugInteractionToolFeature;

public sealed class RegisteredDrugInteractionToolReportTests : IntegrationTestBase
{
    private static readonly Guid NurofenForm200 = Guid.Parse("a1000000-0000-4000-8000-000000000001");
    private static readonly Guid NurofenForm400 = Guid.Parse("a1000000-0000-4000-8000-000000000101");
    private static readonly Guid ControlocForm = Guid.Parse("a1000000-0000-4000-8000-000000000005");
    private static readonly Guid IbuprofenSubstanceId = Guid.Parse("f2000000-0000-4000-8000-000000000001");
    private static readonly Guid AmotaksForm = Guid.Parse("a1000000-0000-4000-8000-000000000006");

    [Fact]
    public async Task Report_WhenPlannedFormSharesDrugWithAllergy_ReturnsPlanTakingOverlapAndSourcedDuplicates()
    {
        JsonElement report = await GetReportAsync([NurofenForm400], showAllFoodAndSupplementInteractions: false);

        JsonElement planAllergies = FindSection(report, "allergies.drug.plan_taking_overlap");
        planAllergies.GetProperty("recordCount").GetInt32().Should().BeGreaterThan(0);
        JsonElement planRow = planAllergies.GetProperty("rows")[0];
        planRow.GetProperty("regularDrugFormId").GetGuid().Should().Be(NurofenForm400);
        planRow.GetProperty("overlapKind").GetInt32().Should().Be(0);

        FindSection(report, "allergies.drug.taken_now_overlap")
            .GetProperty("recordCount").GetInt32().Should().BeGreaterThan(0);

        JsonElement exactDuplicates = FindSection(report, "drug_duplication.exact_active_substance");
        JsonElement ibuprofen = exactDuplicates.GetProperty("rows").EnumerateArray()
            .Single(row => row.GetProperty("activeSubstanceName").GetString() == "Ibuprofen");
        JsonElement[] affectedDrugs = ibuprofen.GetProperty("affectedDrugs").EnumerateArray().ToArray();

        affectedDrugs.Should().HaveCount(2);
        affectedDrugs.Select(drug => drug.GetProperty("source").GetString())
            .Should().BeEquivalentTo("takingNow", "selected");
        affectedDrugs.Should().OnlyContain(drug =>
            drug.GetProperty("onlyDrugSummaryText").GetString()!.StartsWith("Nurofen (Ibuprofen)"));
        affectedDrugs.Select(drug => drug.GetProperty("onlyDrugSummaryText").GetString()!)
            .Should().Contain(text => text.Contains("200 mg"))
            .And.Contain(text => text.Contains("400 mg"));
    }

    [Fact]
    public async Task Report_WhenPlannedFormIsAlreadyTakenNow_ExcludesItFromPlanTakingOverlap()
    {
        JsonElement report = await GetReportAsync([NurofenForm200], showAllFoodAndSupplementInteractions: false);

        FindSection(report, "allergies.drug.plan_taking_overlap")
            .GetProperty("recordCount").GetInt32().Should().Be(0);
        FindSection(report, "allergies.drug.taken_now_overlap")
            .GetProperty("recordCount").GetInt32().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Report_WhenPlannedFormIsAProblematicDrug_ReturnsPlanTakingProblemOverlap()
    {
        JsonElement report = await GetReportAsync([ControlocForm], showAllFoodAndSupplementInteractions: false);

        JsonElement planProblems = FindSection(report, "problematic_drugs.plan_taking_overlap");

        planProblems.GetProperty("recordCount").GetInt32().Should().Be(1);
        planProblems.GetProperty("rows")[0].GetProperty("regularDrugFormId").GetGuid().Should().Be(ControlocForm);
    }

    [Fact]
    public async Task Report_WhenNoManualDrugs_ReturnsEmptyPlanTakingSections()
    {
        JsonElement report = await GetReportAsync([], showAllFoodAndSupplementInteractions: false);

        FindSection(report, "problematic_drugs.plan_taking_overlap")
            .GetProperty("recordCount").GetInt32().Should().Be(0);
        FindSection(report, "allergies.drug.plan_taking_overlap")
            .GetProperty("recordCount").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task Report_AlcoholRow_KeepsSystemDefaultAndCustomTextSeparate()
    {
        JsonElement report = await GetReportAsync([NurofenForm200], showAllFoodAndSupplementInteractions: false);

        JsonElement alcoholRow = FindProperties(report, "alcohol")
            .SelectMany(alcohol => alcohol.GetProperty("rows").EnumerateArray())
            .First(row => row.GetProperty("activeSubstanceName").GetString() == "Ibuprofen");

        string defaultText = alcoholRow.GetProperty("defaultText").GetString()!;
        string? specialText = alcoholRow.GetProperty("specialText").GetString();

        specialText.Should().Contain("Avoid alcohol with Ibuprofen");
        defaultText.Should().NotBe(specialText);
        defaultText.Should().NotContain("Ibuprofen");
    }

    [Fact]
    public async Task Report_FoodRow_ReturnsAliasNamesAndAliasedDisplayLine()
    {
        JsonElement report = await GetReportAsync([AmotaksForm], showAllFoodAndSupplementInteractions: true);

        JsonElement wheatRow = FindProperties(report, "foods")
            .SelectMany(foods => FindProperties(foods, "rows"))
            .SelectMany(rows => rows.EnumerateArray())
            .First(row => row.TryGetProperty("foodName", out JsonElement name) && name.GetString() == "Wheat");

        wheatRow.GetProperty("aliasNames").EnumerateArray().Select(alias => alias.GetString())
            .Should().Equal("gluten", "bread");
        wheatRow.GetProperty("displayLine").GetString().Should().Contain("Wheat (gluten, bread)");
    }

    [Fact]
    public async Task Report_OralGuidance_ReturnsDrugIntakeTimeOnlyForDrugsOnTheRegularList()
    {
        await SeedIbuprofenOralGuidanceAsync();

        JsonElement report = await GetReportAsync(
            [NurofenForm400],
            showAllFoodAndSupplementInteractions: false,
            showOralGuidance: true);

        string?[] intakeTimes = FindProperties(report, "oralGuidance")
            .Where(guidance => guidance.ValueKind == JsonValueKind.Object)
            .Select(guidance => guidance.GetProperty("drugIntakeTime").GetString())
            .ToArray();

        intakeTimes.Should().BeEquivalentTo(new string?[] { "08:00", null });
    }

    private async Task SeedIbuprofenOralGuidanceAsync()
    {
        using IServiceScope scope = Factory.Services.CreateScope();
        DrugsAppDbContext dbContext = scope.ServiceProvider.GetRequiredService<DrugsAppDbContext>();

        ActiveSubstance ibuprofen = await dbContext.ActiveSubstances
            .Include(substance => substance.OralMedicationIntakeGuidelines)
            .SingleAsync(substance => substance.Id == IbuprofenSubstanceId);

        ibuprofen.InitializeOralMedicationIntakeGuidelines(
            FoodIntakeType.WithMeal,
            waitBeforeMinutes: null,
            waitAfterMinutes: null,
            mealNotes: null,
            avoidFoodDrug: null,
            avoidFoodTherapy: null,
            liquidNotes: null,
            liquidInstructions: null,
            recommendedMorning: null,
            recommendedEvening: null);

        await dbContext.SaveChangesAsync();
    }

    private async Task<JsonElement> GetReportAsync(
        Guid[] manualDrugFormIds,
        bool showAllFoodAndSupplementInteractions,
        bool showOralGuidance = false)
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
                showAllDrugsInteractions = true,
                showOralGuidance,
                showAllFoodAndSupplementInteractions
            },
            IntegrationTestJson.ApiRequestOptions,
            CancellationToken.None).WaitAsync(stepTimeout);

        string body = await response.Content.ReadAsStringAsync(CancellationToken.None).WaitAsync(stepTimeout);
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);

        return JsonDocument.Parse(body).RootElement.Clone();
    }

    private static JsonElement FindSection(JsonElement root, string sectionCode) =>
        Descendants(root)
            .First(element =>
                element.ValueKind == JsonValueKind.Object
                && element.TryGetProperty("sectionCode", out JsonElement code)
                && code.ValueKind == JsonValueKind.String
                && code.GetString() == sectionCode);

    private static IEnumerable<JsonElement> FindProperties(JsonElement root, string propertyName) =>
        Descendants(root)
            .Where(element => element.ValueKind == JsonValueKind.Object)
            .Select(element => element.TryGetProperty(propertyName, out JsonElement value)
                ? (JsonElement?)value
                : null)
            .Where(value => value.HasValue)
            .Select(value => value!.Value);

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
