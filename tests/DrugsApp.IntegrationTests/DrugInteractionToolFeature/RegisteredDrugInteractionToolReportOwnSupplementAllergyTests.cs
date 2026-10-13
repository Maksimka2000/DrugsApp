using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.RegularSupplementsFeature.Entities;
using DrugsApp.Domain.RegularSupplementsFeature.Enums;
using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Infrastructure.Persistence;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DrugsApp.IntegrationTests.DrugInteractionToolFeature;

/// <summary>
/// Guards that an allergy to a supplement the user takes through an own supplement is flagged as a
/// taken-now allergy overlap, the same as when the supplement is taken directly.
/// </summary>
public sealed class RegisteredDrugInteractionToolReportOwnSupplementAllergyTests : IntegrationTestBase
{
    [Fact]
    public async Task Report_WhenUserIsAllergicToAnOwnSupplementMember_FlagsTakenNowOverlap()
    {
        Guid supplementId = await SeedAllergyAndOwnSupplementAsync();

        var login = await TestAuthHelper.LoginAsClientAsync(Client, CancellationToken.None);
        TestAuthHelper.ApplyBearerToken(Client, login.AccessToken);

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/tools/drug-interactions/registered/report",
            new
            {
                manualDrugFormIds = Array.Empty<Guid>(),
                includeUserData = true,
                showAllDrugsInteractions = false,
                showOralGuidance = false,
                showAllFoodAndSupplementInteractions = false
            },
            IntegrationTestJson.ApiRequestOptions);
        string body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        JsonElement overlap = JsonDocument.Parse(body).RootElement
            .GetProperty("report")
            .GetProperty("userInfoSection")
            .GetProperty("allergies")
            .GetProperty("supplementAllergies")
            .GetProperty("takenNowOverlap");

        overlap.GetProperty("rows").EnumerateArray()
            .Should().Contain(
                row => row.GetProperty("regularSupplementId").GetGuid() == supplementId,
                because: "the user takes the allergen inside an own supplement");
    }

    private async Task<Guid> SeedAllergyAndOwnSupplementAsync()
    {
        using IServiceScope scope = Factory.Services.CreateScope();
        DrugsAppDbContext dbContext = scope.ServiceProvider.GetRequiredService<DrugsAppDbContext>();
        Guid userId = await dbContext.Users
            .Where(user => user.Email == TestCredentials.Client1Email)
            .Select(user => user.Id)
            .SingleAsync();
        Guid supplementId = await dbContext.Set<Supplement>()
            .AsNoTracking()
            .Where(item => !dbContext.Set<ClientSupplementAllergy>()
                .Any(allergy => allergy.UserId == userId && allergy.SupplementId == item.Id))
            .Where(item => !dbContext.Set<ClientRegularSupplement>()
                .Any(row => row.UserId == userId && row.SupplementId == item.Id))
            .OrderBy(item => item.Id)
            .Select(item => item.Id)
            .FirstAsync();

        dbContext.Set<ClientSupplementAllergy>().Add(
            ClientSupplementAllergy.CreateDatabase(userId, supplementId, "rash").Value);

        ClientOwnSupplement ownSupplement = ClientOwnSupplement.Create(
            userId,
            $"Allergy blend {Guid.NewGuid():N}",
            [supplementId]).Value;
        dbContext.Set<ClientOwnSupplement>().Add(ownSupplement);
        dbContext.Set<ClientRegularSupplement>().Add(ClientRegularSupplement.Create(
            userId,
            RegularSupplementSource.Own,
            supplementId: null,
            ownSupplement.Id,
            startDate: null,
            endDate: null,
            [new TimeOnly(8, 0)],
            DateOnly.FromDateTime(DateTime.UtcNow)).Value);

        await dbContext.SaveChangesAsync();
        return supplementId;
    }
}
