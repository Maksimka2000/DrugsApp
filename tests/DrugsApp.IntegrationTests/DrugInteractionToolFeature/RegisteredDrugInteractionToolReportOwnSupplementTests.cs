using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Enums;
using DrugsApp.Domain.RegularSupplementsFeature.Entities;
using DrugsApp.Domain.RegularSupplementsFeature.Enums;
using DrugsApp.Infrastructure.Persistence;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DrugsApp.IntegrationTests.DrugInteractionToolFeature;

/// <summary>
/// Guards that supplements inside the user's own (custom) supplements are expanded into the registered report:
/// they get their own per-drug subsection naming the own supplement, they leave the "all other" list,
/// and the own supplement is listed with its members in the info section.
/// </summary>
public sealed class RegisteredDrugInteractionToolReportOwnSupplementTests : IntegrationTestBase
{
    [Fact]
    public async Task Report_WhenUserTakesOwnSupplementWithAnInteractingMember_ReportsTheInteractionUnderOwnSupplement()
    {
        (Guid drugFormId, Guid memberSupplementId) = await FindFormAndInteractingSupplementAsync();
        (Guid ownSupplementId, string ownSupplementName) = await SeedActiveOwnSupplementAsync(memberSupplementId);

        JsonElement report = await GetReportAsync([drugFormId], showAllFoodAndSupplementInteractions: true);

        JsonElement[] ownSupplementRows = FindSections(report, "specific_drug.supplements.own_supplement")
            .SelectMany(section => section.GetProperty("rows").EnumerateArray())
            .ToArray();

        ownSupplementRows.Should().Contain(
            row => row.GetProperty("ownSupplementId").GetGuid() == ownSupplementId
                && row.GetProperty("supplementId").GetGuid() == memberSupplementId,
            because: "the member supplement interacts with the selected drug");

        JsonElement ownRow = ownSupplementRows.First(row => row.GetProperty("ownSupplementId").GetGuid() == ownSupplementId);
        ownRow.GetProperty("ownSupplementName").GetString().Should().Be(ownSupplementName);
        ownRow.GetProperty("isTakenNow").GetBoolean().Should().BeTrue();
        ownRow.GetProperty("displayLine").GetString().Should().Contain($"in own supplement ({ownSupplementName})");

        FindSections(report, "specific_drug.supplements.all_other")
            .SelectMany(section => section.GetProperty("rows").EnumerateArray())
            .Should().NotContain(
                row => row.GetProperty("supplementId").GetGuid() == memberSupplementId,
                because: "a supplement taken through an own supplement is not an interaction the user does not take");

        JsonElement infoOwnSupplement = report
            .GetProperty("report")
            .GetProperty("info")
            .GetProperty("supplements")
            .GetProperty("ownSupplements")
            .GetProperty("rows")
            .EnumerateArray()
            .Single(row => row.GetProperty("ownSupplementId").GetGuid() == ownSupplementId);

        infoOwnSupplement.GetProperty("ownSupplementName").GetString().Should().Be(ownSupplementName);
        infoOwnSupplement.GetProperty("memberSupplements").EnumerateArray()
            .Select(member => member.GetProperty("supplementId").GetGuid())
            .Should().Contain(memberSupplementId);
    }

    [Fact]
    public async Task Report_WhenOnlyUserDataIsRequested_StillLoadsInteractionsOfOwnSupplementMembers()
    {
        (Guid drugFormId, Guid memberSupplementId) = await FindFormAndInteractingSupplementAsync();
        (Guid ownSupplementId, _) = await SeedActiveOwnSupplementAsync(memberSupplementId);

        JsonElement report = await GetReportAsync([drugFormId], showAllFoodAndSupplementInteractions: false);

        FindSections(report, "specific_drug.supplements.own_supplement")
            .SelectMany(section => section.GetProperty("rows").EnumerateArray())
            .Should().Contain(
                row => row.GetProperty("ownSupplementId").GetGuid() == ownSupplementId,
                because: "own supplement members must be part of the scoped bulk interaction query");
    }

    private async Task<(Guid DrugFormId, Guid SupplementId)> FindFormAndInteractingSupplementAsync()
    {
        using IServiceScope scope = Factory.Services.CreateScope();
        DrugsAppDbContext dbContext = scope.ServiceProvider.GetRequiredService<DrugsAppDbContext>();

        List<InteractionExpanded> supplementRows = await dbContext.InteractionExpandedRows
            .AsNoTracking()
            .Where(candidate => candidate.PartyTypeA == ShortInteractionType.Substance
                && candidate.PartyTypeB == ShortInteractionType.Supplement)
            .OrderBy(candidate => candidate.Id)
            .ToListAsync();

        foreach (InteractionExpanded row in supplementRows)
        {
            Guid? drugFormId = await dbContext.Set<DrugForm>()
                .AsNoTracking()
                .Where(form => form.Drug.ActiveSubstances.Any(substance => substance.Id == row.PartyIdA)
                    && form.Drug.TypeOfDrug == DrugType.Standard
                    && form.Drug.TypeOfProduct != "Weterynaryjny")
                .OrderBy(form => form.Id)
                .Select(form => (Guid?)form.Id)
                .FirstOrDefaultAsync();

            if (drugFormId.HasValue)
            {
                return (drugFormId.Value, row.PartyIdB);
            }
        }

        throw new InvalidOperationException("The seed has no eligible drug form with a supplement interaction.");
    }

    private async Task<(Guid OwnSupplementId, string OwnSupplementName)> SeedActiveOwnSupplementAsync(
        Guid memberSupplementId)
    {
        using IServiceScope scope = Factory.Services.CreateScope();
        DrugsAppDbContext dbContext = scope.ServiceProvider.GetRequiredService<DrugsAppDbContext>();
        Guid userId = await dbContext.Users
            .Where(user => user.Email == TestCredentials.Client1Email)
            .Select(user => user.Id)
            .SingleAsync();
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        string ownSupplementName = $"Integration own supplement {Guid.NewGuid():N}";

        ClientOwnSupplement ownSupplement = ClientOwnSupplement.Create(
            userId,
            ownSupplementName,
            [memberSupplementId]).Value;
        dbContext.Set<ClientOwnSupplement>().Add(ownSupplement);

        dbContext.Set<ClientRegularSupplement>().Add(ClientRegularSupplement.Create(
            userId,
            RegularSupplementSource.Own,
            supplementId: null,
            ownSupplement.Id,
            startDate: null,
            endDate: null,
            [new TimeOnly(8, 0)],
            today).Value);

        await dbContext.SaveChangesAsync();

        return (ownSupplement.Id, ownSupplement.Name);
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
