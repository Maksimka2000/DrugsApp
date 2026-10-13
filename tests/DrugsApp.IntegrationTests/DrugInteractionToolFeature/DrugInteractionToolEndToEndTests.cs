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
/// End-to-end checks of both drug interaction tool endpoints: every flag combination must return a report
/// that is internally consistent and byte-for-byte deterministic, and invalid input must fail with the documented errors.
/// </summary>
public sealed class DrugInteractionToolEndToEndTests : IntegrationTestBase
{
    private static readonly Guid Nurofen200 = Guid.Parse("a1000000-0000-4000-8000-000000000001");
    private static readonly Guid Nurofen400 = Guid.Parse("a1000000-0000-4000-8000-000000000101");
    private static readonly Guid Controloc = Guid.Parse("a1000000-0000-4000-8000-000000000005");
    private static readonly Guid Amotaks = Guid.Parse("a1000000-0000-4000-8000-000000000006");
    private static readonly Guid OphthalmicForm = Guid.Parse("a1000000-0000-4000-8000-000000000021");
    private static readonly Guid SpecialForm = Guid.Parse("a1000000-0000-4000-8000-000000000026");

    private static readonly bool[] Both = [false, true];

    [Fact]
    public async Task PublicReport_AcrossEveryFlagCombination_IsConsistentAndDeterministic()
    {
        (Guid firstInteracting, Guid secondInteracting) = await FindInteractingDrugFormsAsync();
        Guid[] manualForms = new[] { firstInteracting, secondInteracting, Amotaks }.Distinct().ToArray();

        foreach (bool showAllDrugs in Both)
        foreach (bool showOralGuidance in Both)
        foreach (bool showAllFoodAndSupplements in Both)
        {
            object request = new
            {
                manualDrugFormIds = manualForms,
                showAllDrugsInteractions = showAllDrugs,
                showOralGuidance,
                showAllFoodAndSupplementInteractions = showAllFoodAndSupplements
            };
            string context = $"public showAllDrugs={showAllDrugs} oral={showOralGuidance} foods={showAllFoodAndSupplements}";

            (HttpStatusCode status, string firstBody) = await PostAsync("/api/tools/drug-interactions/public/report", request);
            status.Should().Be(HttpStatusCode.OK, because: $"{context}: {firstBody}");
            (_, string secondBody) = await PostAsync("/api/tools/drug-interactions/public/report", request);
            secondBody.Should().Be(firstBody, because: $"{context}: the same request must give the same bytes");

            JsonElement report = JsonDocument.Parse(firstBody).RootElement.GetProperty("report");
            AssertReportIsConsistent(report, manualForms.Length, showAllDrugs, context);

            ReportDrugFormIds(report)
                .Should().BeEquivalentTo(manualForms, because: context);

            if (showAllDrugs)
            {
                report.GetProperty("allDrugsInteractionsSection").GetProperty("recordCount").GetInt32()
                    .Should().BeGreaterThan(0, because: $"{context}: the scenario must contain a real drug-drug interaction");
            }

            if (showAllFoodAndSupplements)
            {
                Descendants(report)
                    .Where(element => element.ValueKind == JsonValueKind.Object
                        && element.TryGetProperty("foodName", out _))
                    .Should().NotBeEmpty(because: $"{context}: the scenario must contain a real food interaction");
            }

            if (!showAllFoodAndSupplements)
            {
                foreach (JsonElement specific in report.GetProperty("specificDrugReports").EnumerateArray())
                {
                    IsAbsent(specific, "foods").Should().BeTrue(because: $"{context}: foods hidden");
                    IsAbsent(specific, "supplements").Should().BeTrue(because: $"{context}: supplements hidden");
                }
            }
        }
    }

    [Fact]
    public async Task RegisteredReport_AcrossEveryFlagCombination_IsConsistentAndDeterministic()
    {
        (Guid firstInteracting, Guid secondInteracting) = await FindInteractingDrugFormsAsync();
        Guid[] manualForms = new[] { firstInteracting, secondInteracting, Controloc, Amotaks }.Distinct().ToArray();
        await LoginAsClientAsync();

        foreach (bool includeUserData in Both)
        foreach (bool showAllDrugs in Both)
        foreach (bool showOralGuidance in Both)
        foreach (bool showAllFoodAndSupplements in Both)
        {
            object request = new
            {
                manualDrugFormIds = manualForms,
                includeUserData,
                showAllDrugsInteractions = showAllDrugs,
                showOralGuidance,
                showAllFoodAndSupplementInteractions = showAllFoodAndSupplements
            };
            string context =
                $"registered userData={includeUserData} showAllDrugs={showAllDrugs} oral={showOralGuidance} foods={showAllFoodAndSupplements}";

            (HttpStatusCode status, string firstBody) = await PostAsync("/api/tools/drug-interactions/registered/report", request);
            status.Should().Be(HttpStatusCode.OK, because: $"{context}: {firstBody}");
            (_, string secondBody) = await PostAsync("/api/tools/drug-interactions/registered/report", request);
            secondBody.Should().Be(firstBody, because: $"{context}: the same request must give the same bytes");

            JsonElement report = JsonDocument.Parse(firstBody).RootElement.GetProperty("report");
            int expectedDrugCount = ReportDrugFormIds(report).Count;
            AssertReportIsConsistent(report, expectedDrugCount, showAllDrugs, context);

            if (showAllDrugs)
            {
                report.GetProperty("allDrugsInteractionsSection").GetProperty("recordCount").GetInt32()
                    .Should().BeGreaterThan(0, because: $"{context}: the scenario must contain a real drug-drug interaction");
            }

            ReportDrugFormIds(report)
                .Should().Contain(manualForms, because: $"{context}: manual forms are always reported");

            if (includeUserData)
            {
                IsAbsent(report, "userInfoSection").Should().BeFalse(because: $"{context}: user data requested");
                expectedDrugCount.Should().BeGreaterThan(manualForms.Length, because: $"{context}: active regular drugs merged");
            }
            else
            {
                IsAbsent(report, "userInfoSection").Should().BeTrue(because: $"{context}: user data not requested");
                expectedDrugCount.Should().Be(manualForms.Length, because: $"{context}: nothing merged");
            }
        }
    }

    [Fact]
    public async Task Report_WhenTwoFormsOfTheSameDrugAreSelected_NeverPairsThemWithEachOther()
    {
        (_, string body) = await PostAsync(
            "/api/tools/drug-interactions/public/report",
            new
            {
                manualDrugFormIds = new[] { Nurofen200, Nurofen400, Amotaks },
                showAllDrugsInteractions = true,
                showOralGuidance = false,
                showAllFoodAndSupplementInteractions = false
            });

        JsonElement report = JsonDocument.Parse(body).RootElement.GetProperty("report");

        foreach (JsonElement specific in report.GetProperty("specificDrugReports").EnumerateArray())
        {
            Guid ownDrugId = specific.GetProperty("drug").GetProperty("drugId").GetGuid();

            specific.GetProperty("drugs").GetProperty("comparedDrugGroups").EnumerateArray()
                .Select(group => group.GetProperty("comparedDrug").GetProperty("drugId").GetGuid())
                .Should().NotContain(ownDrugId);
        }
    }

    [Fact]
    public async Task PublicReport_WithInvalidInput_FailsWithTheDocumentedErrors()
    {
        Guid unknownFormId = Guid.NewGuid();
        (HttpStatusCode unknownStatus, string unknownBody) = await PostAsync(
            "/api/tools/drug-interactions/public/report",
            PublicRequest(unknownFormId));
        unknownStatus.Should().Be(HttpStatusCode.BadRequest, because: unknownBody);
        unknownBody.Should().Contain("were not found").And.Contain(unknownFormId.ToString());

        (HttpStatusCode ophthalmicStatus, string ophthalmicBody) = await PostAsync(
            "/api/tools/drug-interactions/public/report",
            PublicRequest(OphthalmicForm));
        ophthalmicStatus.Should().Be(HttpStatusCode.BadRequest, because: ophthalmicBody);
        ophthalmicBody.Should().Contain("not eligible").And.Contain(OphthalmicForm.ToString());

        (HttpStatusCode specialStatus, string specialBody) = await PostAsync(
            "/api/tools/drug-interactions/public/report",
            PublicRequest(SpecialForm));
        specialStatus.Should().Be(HttpStatusCode.BadRequest, because: specialBody);
        specialBody.Should().Contain("not eligible").And.Contain(SpecialForm.ToString());

        (HttpStatusCode tooManyStatus, _) = await PostAsync(
            "/api/tools/drug-interactions/public/report",
            PublicRequest(Nurofen200, Nurofen400, Amotaks, Controloc));
        tooManyStatus.Should().Be(HttpStatusCode.BadRequest);

        (HttpStatusCode duplicateStatus, _) = await PostAsync(
            "/api/tools/drug-interactions/public/report",
            PublicRequest(Nurofen200, Nurofen200));
        duplicateStatus.Should().Be(HttpStatusCode.BadRequest);

        (HttpStatusCode emptyStatus, string emptyBody) = await PostAsync(
            "/api/tools/drug-interactions/public/report",
            PublicRequest());
        emptyStatus.Should().Be(HttpStatusCode.OK, because: emptyBody);
        JsonDocument.Parse(emptyBody).RootElement.GetProperty("report")
            .GetProperty("info").GetProperty("drugs").GetProperty("selected")
            .GetProperty("recordCount").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task RegisteredReport_WithoutALoginOrWithInvalidInput_FailsWithTheDocumentedErrors()
    {
        object request = new
        {
            manualDrugFormIds = new[] { Nurofen200 },
            includeUserData = true,
            showAllDrugsInteractions = true,
            showOralGuidance = false,
            showAllFoodAndSupplementInteractions = false
        };

        (HttpStatusCode anonymousStatus, _) = await PostAsync("/api/tools/drug-interactions/registered/report", request);
        anonymousStatus.Should().Be(HttpStatusCode.Unauthorized);

        await LoginAsClientAsync();

        Guid unknownFormId = Guid.NewGuid();
        (HttpStatusCode unknownStatus, string unknownBody) = await PostAsync(
            "/api/tools/drug-interactions/registered/report",
            new
            {
                manualDrugFormIds = new[] { unknownFormId },
                includeUserData = true,
                showAllDrugsInteractions = true,
                showOralGuidance = false,
                showAllFoodAndSupplementInteractions = false
            });
        unknownStatus.Should().Be(HttpStatusCode.BadRequest, because: unknownBody);
        unknownBody.Should().Contain("were not found").And.Contain(unknownFormId.ToString());
    }

    private async Task<(Guid First, Guid Second)> FindInteractingDrugFormsAsync()
    {
        using IServiceScope scope = Factory.Services.CreateScope();
        DrugsAppDbContext dbContext = scope.ServiceProvider.GetRequiredService<DrugsAppDbContext>();

        InteractionExpanded substancePair = await dbContext.InteractionExpandedRows
            .AsNoTracking()
            .Where(row => row.PartyTypeA == ShortInteractionType.Substance
                && row.PartyTypeB == ShortInteractionType.Substance)
            .OrderBy(row => row.Id)
            .FirstAsync();

        (Guid FormId, Guid DrugId) first = await FindEligibleFormContainingAsync(dbContext, substancePair.PartyIdA, null);
        (Guid FormId, Guid DrugId) second = await FindEligibleFormContainingAsync(dbContext, substancePair.PartyIdB, first.DrugId);

        return (first.FormId, second.FormId);
    }

    private static async Task<(Guid FormId, Guid DrugId)> FindEligibleFormContainingAsync(
        DrugsAppDbContext dbContext,
        Guid substanceId,
        Guid? excludedDrugId)
    {
        var form = await dbContext.Set<DrugForm>()
            .AsNoTracking()
            .Where(candidate => candidate.Drug.ActiveSubstances.Any(substance => substance.Id == substanceId)
                && candidate.Drug.TypeOfDrug == DrugType.Standard
                && candidate.Drug.TypeOfProduct != "Weterynaryjny"
                && (excludedDrugId == null || candidate.Drug.Id != excludedDrugId))
            .OrderBy(candidate => candidate.Id)
            .Select(candidate => new { candidate.Id, DrugId = candidate.Drug.Id })
            .FirstAsync();

        return (form.Id, form.DrugId);
    }

    private static object PublicRequest(params Guid[] manualDrugFormIds) => new
    {
        manualDrugFormIds,
        showAllDrugsInteractions = true,
        showOralGuidance = false,
        showAllFoodAndSupplementInteractions = false
    };

    private async Task LoginAsClientAsync()
    {
        var login = await TestAuthHelper.LoginAsClientAsync(Client, CancellationToken.None)
            .WaitAsync(TimeSpan.FromSeconds(60));
        TestAuthHelper.ApplyBearerToken(Client, login.AccessToken);
    }

    private async Task<(HttpStatusCode Status, string Body)> PostAsync(string url, object request)
    {
        TimeSpan stepTimeout = TimeSpan.FromSeconds(60);

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            url,
            request,
            IntegrationTestJson.ApiRequestOptions,
            CancellationToken.None).WaitAsync(stepTimeout);

        string body = await response.Content.ReadAsStringAsync(CancellationToken.None).WaitAsync(stepTimeout);
        return (response.StatusCode, body);
    }

    private static List<Guid> ReportDrugFormIds(JsonElement report)
    {
        JsonElement drugs = report.GetProperty("info").GetProperty("drugs");

        return new[] { "selected", "takenNow" }
            .Select(subsectionName => drugs.GetProperty(subsectionName))
            .Where(subsection => subsection.ValueKind == JsonValueKind.Object)
            .SelectMany(subsection => subsection.GetProperty("rows").EnumerateArray())
            .Select(row => row.GetProperty("drugFormId").GetGuid())
            .Distinct()
            .ToList();
    }

    private static void AssertReportIsConsistent(
        JsonElement report,
        int expectedDrugCount,
        bool showAllDrugs,
        string context)
    {
        JsonElement specificReports = report.GetProperty("specificDrugReports");
        ReportDrugFormIds(report).Should().HaveCount(expectedDrugCount, because: context);
        specificReports.GetArrayLength().Should().Be(expectedDrugCount, because: context);

        foreach (JsonElement subsection in Descendants(report).Where(IsCountedSubsection))
        {
            subsection.GetProperty("recordCount").GetInt32()
                .Should().Be(subsection.GetProperty("rows").GetArrayLength(), because: $"{context}: recordCount matches rows");
        }

        Dictionary<(Guid Current, Guid Compared), int> pairCounts = new();

        foreach (JsonElement specific in specificReports.EnumerateArray())
        {
            Guid currentFormId = specific.GetProperty("drug").GetProperty("selectedDrugFormId").GetGuid();
            JsonElement drugs = specific.GetProperty("drugs");
            int groupTotal = 0;

            foreach (JsonElement group in drugs.GetProperty("comparedDrugGroups").EnumerateArray())
            {
                int rows = group.GetProperty("pairRows").GetArrayLength();
                group.GetProperty("recordCount").GetInt32().Should().Be(rows, because: $"{context}: group count");
                rows.Should().BeGreaterThan(0, because: $"{context}: empty groups are never returned");
                groupTotal += rows;
                pairCounts[(currentFormId, group.GetProperty("comparedDrug").GetProperty("drugFormId").GetGuid())] = rows;
            }

            drugs.GetProperty("recordCount").GetInt32().Should().Be(groupTotal, because: $"{context}: per-drug total");
        }

        foreach (((Guid current, Guid compared), int rows) in pairCounts)
        {
            pairCounts.Should().ContainKey((compared, current), because: $"{context}: interactions are reported in both directions");
            pairCounts[(compared, current)].Should().Be(rows, because: $"{context}: both directions have the same rows");
        }

        if (!showAllDrugs)
        {
            IsAbsent(report, "allDrugsInteractionsSection").Should().BeTrue(because: $"{context}: matrix hidden");
        }
        else
        {
            JsonElement matrix = report.GetProperty("allDrugsInteractionsSection");
            int matrixTotal = 0;

            foreach (JsonElement pairGroup in matrix.GetProperty("pairGroups").EnumerateArray())
            {
                Guid left = pairGroup.GetProperty("leftDrug").GetProperty("drugFormId").GetGuid();
                Guid right = pairGroup.GetProperty("rightDrug").GetProperty("drugFormId").GetGuid();
                int rows = pairGroup.GetProperty("pairRows").GetArrayLength();
                pairGroup.GetProperty("recordCount").GetInt32().Should().Be(rows, because: context);
                pairCounts.GetValueOrDefault((left, right)).Should().Be(rows, because: $"{context}: matrix equals per-drug view");
                matrixTotal += rows;
            }

            matrix.GetProperty("recordCount").GetInt32().Should().Be(matrixTotal, because: $"{context}: matrix total");
            matrixTotal.Should().Be(pairCounts.Values.Sum() / 2, because: $"{context}: each pair counted once in the matrix");
        }

        AssertDuplicationSection(report.GetProperty("drugDuplicationSection"), context);
    }

    private static void AssertDuplicationSection(JsonElement section, string context)
    {
        foreach (JsonElement row in section.GetProperty("exactActiveSubstanceDuplicates").GetProperty("rows").EnumerateArray())
        {
            DistinctDrugIds(row).Should().HaveCountGreaterThan(1, because: $"{context}: exact rows span different drugs");
        }

        foreach (JsonElement row in section.GetProperty("activeSubstanceGroupDuplicates").GetProperty("rows").EnumerateArray())
        {
            DistinctDrugIds(row).Should().HaveCountGreaterThan(1, because: $"{context}: group rows span different drugs");
        }

        foreach (JsonElement row in section.GetProperty("drugDuplicates").GetProperty("rows").EnumerateArray())
        {
            DistinctDrugIds(row).Should().HaveCount(1, because: $"{context}: one drug per drug row");
            row.GetProperty("affectedDrugs").EnumerateArray()
                .Select(drug => drug.GetProperty("drugFormId").GetGuid())
                .Distinct().Should().HaveCountGreaterThan(1, because: $"{context}: several forms per drug row");
        }

        foreach (JsonElement row in section.GetProperty("drugFormDuplicates").GetProperty("rows").EnumerateArray())
        {
            row.GetProperty("affectedDrugs").EnumerateArray()
                .Select(drug => drug.GetProperty("source").GetString())
                .Should().BeEquivalentTo("takingNow", "selected");
        }
    }

    private static IEnumerable<Guid> DistinctDrugIds(JsonElement row) =>
        row.GetProperty("affectedDrugs").EnumerateArray()
            .Select(drug => drug.GetProperty("drugId").GetGuid())
            .Distinct()
            .ToList();

    private static bool IsCountedSubsection(JsonElement element) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty("recordCount", out JsonElement count)
        && count.ValueKind == JsonValueKind.Number
        && element.TryGetProperty("rows", out JsonElement rows)
        && rows.ValueKind == JsonValueKind.Array;

    private static bool IsAbsent(JsonElement parent, string propertyName) =>
        !parent.TryGetProperty(propertyName, out JsonElement value) || value.ValueKind == JsonValueKind.Null;

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
