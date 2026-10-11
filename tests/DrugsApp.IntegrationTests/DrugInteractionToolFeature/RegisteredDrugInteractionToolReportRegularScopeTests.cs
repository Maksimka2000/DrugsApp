using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Enums;
using DrugsApp.Domain.InteractionFeature.Repositories;
using DrugsApp.Domain.RegularDrugsFeature.Entities;
using DrugsApp.Domain.RegularDrugsFeature.Interfaces;
using DrugsApp.Domain.RegularFoodFeature.Entities;
using DrugsApp.Domain.RegularFoodFeature.Interfaces;
using DrugsApp.Domain.RegularSupplementsFeature.Entities;
using DrugsApp.Domain.RegularSupplementsFeature.Enums;
using DrugsApp.Domain.RegularSupplementsFeature.Repositories;
using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Infrastructure.Persistence;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DrugsApp.IntegrationTests.DrugInteractionToolFeature;

public sealed class RegisteredDrugInteractionToolReportRegularScopeTests : IntegrationTestBase
{
    private static readonly Guid AmotaksDrug = Guid.Parse("a0000000-0000-4000-8000-000000000006");
    private static readonly Guid AmotaksForm = Guid.Parse("a1000000-0000-4000-8000-000000000006");

    [Fact]
    public async Task Report_WhenRegularDrugIsScheduled_DoesNotIncludeItInTheReport()
    {
        await SeedRegularDrugAsync(AmotaksDrug, AmotaksForm, startDateOffsetDays: 30);

        JsonElement report = await GetReportAsync(manualDrugFormIds: []);

        SelectedDrugFormIds(report).Should().NotContain(AmotaksForm);
    }

    [Fact]
    public async Task Report_WhenRegularDrugIsActive_IncludesItAsUserRegularWithRoutesOfAdministration()
    {
        await SeedRegularDrugAsync(AmotaksDrug, AmotaksForm, startDateOffsetDays: 0);

        JsonElement report = await GetReportAsync(manualDrugFormIds: []);

        SelectedDrugFormIds(report).Should().Contain(AmotaksForm);

        JsonElement drug = FindProperties(report, "specificDrugReports")
            .Single()
            .EnumerateArray()
            .Select(specificReport => specificReport.GetProperty("drug"))
            .Single(detail => detail.GetProperty("selectedDrugFormId").GetGuid() == AmotaksForm);

        drug.GetProperty("selectedDrugSource").GetString().Should().Be("user_regular");
        drug.GetProperty("selectedDrugFormSummary")
            .GetProperty("routesOfAdministration")
            .GetArrayLength()
            .Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ListActiveFoodIds_ReturnsOnlyActiveRows()
    {
        using IServiceScope scope = Factory.Services.CreateScope();
        DrugsAppDbContext dbContext = scope.ServiceProvider.GetRequiredService<DrugsAppDbContext>();
        Guid userId = await GetClientUserIdAsync(dbContext);
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

        List<Guid> unusedFoodIds = await dbContext.Set<Food>()
            .Where(food => !dbContext.Set<ClientRegularFood>()
                .Any(row => row.UserId == userId && row.FoodId == food.Id))
            .Select(food => food.Id)
            .Take(2)
            .ToListAsync();
        unusedFoodIds.Should().HaveCount(2);

        Guid activeFoodId = unusedFoodIds[0];
        Guid scheduledFoodId = unusedFoodIds[1];

        dbContext.Set<ClientRegularFood>().Add(ClientRegularFood.Create(
            userId, activeFoodId, startDate: null, endDate: null, [new TimeOnly(8, 0)], today).Value);
        dbContext.Set<ClientRegularFood>().Add(ClientRegularFood.Create(
            userId, scheduledFoodId, today.AddDays(30), endDate: null, [new TimeOnly(8, 0)], today).Value);
        await dbContext.SaveChangesAsync();

        IClientRegularFoodRepository repository = scope.ServiceProvider.GetRequiredService<IClientRegularFoodRepository>();
        List<Guid> activeFoodIds = await repository.ListActiveFoodIdsByUserIdAsync(userId, CancellationToken.None);

        activeFoodIds.Should().Contain(activeFoodId);
        activeFoodIds.Should().NotContain(scheduledFoodId);
    }

    [Fact]
    public async Task ListActiveDatabaseSupplementIds_ReturnsOnlyActiveDatabaseRows()
    {
        using IServiceScope scope = Factory.Services.CreateScope();
        DrugsAppDbContext dbContext = scope.ServiceProvider.GetRequiredService<DrugsAppDbContext>();
        Guid userId = await GetClientUserIdAsync(dbContext);
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

        List<Guid> unusedSupplementIds = await dbContext.Set<Supplement>()
            .Where(supplement => !dbContext.Set<ClientRegularSupplement>()
                .Any(row => row.UserId == userId && row.SupplementId == supplement.Id))
            .Select(supplement => supplement.Id)
            .Take(2)
            .ToListAsync();
        unusedSupplementIds.Should().HaveCount(2);

        Guid activeSupplementId = unusedSupplementIds[0];
        Guid scheduledSupplementId = unusedSupplementIds[1];

        dbContext.Set<ClientRegularSupplement>().Add(ClientRegularSupplement.Create(
            userId,
            RegularSupplementSource.Database,
            activeSupplementId,
            ownSupplementId: null,
            startDate: null,
            endDate: null,
            [new TimeOnly(8, 0)],
            today).Value);
        dbContext.Set<ClientRegularSupplement>().Add(ClientRegularSupplement.Create(
            userId,
            RegularSupplementSource.Database,
            scheduledSupplementId,
            ownSupplementId: null,
            today.AddDays(30),
            endDate: null,
            [new TimeOnly(8, 0)],
            today).Value);
        await dbContext.SaveChangesAsync();

        IClientRegularSupplementRepository repository =
            scope.ServiceProvider.GetRequiredService<IClientRegularSupplementRepository>();
        List<Guid> activeSupplementIds =
            await repository.ListActiveDatabaseSupplementIdsByUserIdAsync(userId, CancellationToken.None);

        activeSupplementIds.Should().Contain(activeSupplementId);
        activeSupplementIds.Should().NotContain(scheduledSupplementId);
    }

    [Fact]
    public async Task SearchExpandedRowsBulk_ScopesFoodRowsToTheRequestedIds()
    {
        using IServiceScope scope = Factory.Services.CreateScope();
        DrugsAppDbContext dbContext = scope.ServiceProvider.GetRequiredService<DrugsAppDbContext>();

        InteractionExpanded anyFoodRow = await dbContext.InteractionExpandedRows
            .AsNoTracking()
            .Where(row => row.PartyTypeA == ShortInteractionType.Substance
                && row.PartyTypeB == ShortInteractionType.Food)
            .OrderBy(row => row.Id)
            .FirstAsync();

        Guid[] substanceIds = [anyFoodRow.PartyIdA];
        IInteractionRepository repository = scope.ServiceProvider.GetRequiredService<IInteractionRepository>();

        List<InteractionExpanded> everyFood = await repository.SearchExpandedRowsBulkAsync(
            substanceIds, includeAllFood: true, takenNowFoodIds: null,
            includeAllSupplements: false, takenNowSupplementIds: null, CancellationToken.None);
        List<InteractionExpanded> scoped = await repository.SearchExpandedRowsBulkAsync(
            substanceIds, includeAllFood: false, takenNowFoodIds: [anyFoodRow.PartyIdB],
            includeAllSupplements: false, takenNowSupplementIds: null, CancellationToken.None);
        List<InteractionExpanded> unrelatedScope = await repository.SearchExpandedRowsBulkAsync(
            substanceIds, includeAllFood: false, takenNowFoodIds: [Guid.NewGuid()],
            includeAllSupplements: false, takenNowSupplementIds: null, CancellationToken.None);
        List<InteractionExpanded> noFood = await repository.SearchExpandedRowsBulkAsync(
            substanceIds, includeAllFood: false, takenNowFoodIds: null,
            includeAllSupplements: false, takenNowSupplementIds: null, CancellationToken.None);

        everyFood.Should().Contain(row => row.Id == anyFoodRow.Id);
        scoped.Should().NotBeEmpty();
        scoped.Should().OnlyContain(row =>
            row.PartyTypeB == ShortInteractionType.Food && row.PartyIdB == anyFoodRow.PartyIdB);
        unrelatedScope.Should().NotContain(row => row.PartyTypeB == ShortInteractionType.Food);
        noFood.Should().NotContain(row => row.PartyTypeB == ShortInteractionType.Food);
    }

    private async Task SeedRegularDrugAsync(Guid drugId, Guid drugFormId, int startDateOffsetDays)
    {
        using IServiceScope scope = Factory.Services.CreateScope();
        DrugsAppDbContext dbContext = scope.ServiceProvider.GetRequiredService<DrugsAppDbContext>();
        Guid userId = await GetClientUserIdAsync(dbContext);
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

        DateOnly? startDate = startDateOffsetDays == 0 ? null : today.AddDays(startDateOffsetDays);

        ClientRegularDrug regularDrug = ClientRegularDrug.Create(
            userId,
            drugId,
            drugFormId,
            startDate,
            endDate: null,
            [new TimeOnly(8, 0)],
            today).Value;

        dbContext.Set<ClientRegularDrug>().Add(regularDrug);
        await dbContext.SaveChangesAsync();
    }

    private static async Task<Guid> GetClientUserIdAsync(DrugsAppDbContext dbContext) =>
        await dbContext.Users
            .Where(user => user.Email == TestCredentials.Client1Email)
            .Select(user => user.Id)
            .SingleAsync();

    private static IEnumerable<Guid> SelectedDrugFormIds(JsonElement report)
    {
        JsonElement drugs = report.GetProperty("report").GetProperty("info").GetProperty("drugs");

        return new[] { "selected", "takenNow" }
            .Select(subsectionName => drugs.GetProperty(subsectionName))
            .Where(subsection => subsection.ValueKind == JsonValueKind.Object)
            .SelectMany(subsection => subsection.GetProperty("rows").EnumerateArray())
            .Select(row => row.GetProperty("drugFormId").GetGuid())
            .Distinct()
            .ToList();
    }

    private async Task<JsonElement> GetReportAsync(Guid[] manualDrugFormIds)
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
                showOralGuidance = false,
                showAllFoodAndSupplementInteractions = false
            },
            IntegrationTestJson.ApiRequestOptions,
            CancellationToken.None).WaitAsync(stepTimeout);

        string body = await response.Content.ReadAsStringAsync(CancellationToken.None).WaitAsync(stepTimeout);
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);

        return JsonDocument.Parse(body).RootElement.Clone();
    }

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
