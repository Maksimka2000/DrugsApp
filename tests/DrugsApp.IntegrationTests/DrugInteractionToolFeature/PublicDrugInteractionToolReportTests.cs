using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.DrugInteractionToolFeature;

public sealed class PublicDrugInteractionToolReportTests : IntegrationTestBase
{
    private static readonly Guid NurofenForm200 = Guid.Parse("a1000000-0000-4000-8000-000000000001");
    private static readonly Guid NurofenForm400 = Guid.Parse("a1000000-0000-4000-8000-000000000101");

    [Fact]
    public async Task Report_WhenTwoFormsOfOneDrugAreSelected_ReportsADrugDuplicateAndNoSubstanceDuplicate()
    {
        JsonElement report = await GetReportAsync([NurofenForm200, NurofenForm400]);

        JsonElement section = report.GetProperty("report").GetProperty("drugDuplicationSection");

        JsonElement drugRow = section.GetProperty("drugDuplicates").GetProperty("rows").EnumerateArray().Single();
        drugRow.GetProperty("drugName").GetString().Should().Be("Nurofen");
        drugRow.GetProperty("affectedDrugs").EnumerateArray()
            .Select(drug => drug.GetProperty("drugFormId").GetGuid())
            .Should().BeEquivalentTo(new[] { NurofenForm200, NurofenForm400 });

        section.GetProperty("exactActiveSubstanceDuplicates").GetProperty("recordCount").GetInt32().Should().Be(0);
        section.GetProperty("activeSubstanceGroupDuplicates").GetProperty("recordCount").GetInt32().Should().Be(0);
        section.GetProperty("drugFormDuplicates").GetProperty("recordCount").GetInt32().Should().Be(0);
    }

    private async Task<JsonElement> GetReportAsync(Guid[] manualDrugFormIds)
    {
        TimeSpan stepTimeout = TimeSpan.FromSeconds(60);

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/tools/drug-interactions/public/report",
            new
            {
                manualDrugFormIds,
                showAllDrugsInteractions = true,
                showOralGuidance = false,
                showAllFoodAndSupplementInteractions = false
            },
            IntegrationTestJson.ApiRequestOptions,
            CancellationToken.None).WaitAsync(stepTimeout);

        string body = await response.Content.ReadAsStringAsync(CancellationToken.None).WaitAsync(stepTimeout);
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);

        return JsonDocument.Parse(body).RootElement.Clone();
    }
}
