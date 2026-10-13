using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Infrastructure.Persistence;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DrugsApp.IntegrationTests.RegularSupplementsFeature;

/// <summary>
/// Guards that an own (custom) supplement can be added to the regular supplements list.
/// </summary>
public sealed class CreateRegularSupplementFromOwnSupplementTests : IntegrationTestBase
{
    [Fact]
    public async Task Create_WhenSourceIsOwn_Returns201WithMembersAndLeavesTheOwnSupplementUntouched()
    {
        List<Guid> memberIds = await GetSupplementIdsAsync(count: 2);
        await SignInAsClientAsync();

        JsonElement ownSupplement = await PostJsonAsync(
            "/api/own-supplements",
            new { name = "Integration protein", supplementIds = memberIds },
            HttpStatusCode.Created);
        Guid ownSupplementId = ownSupplement.GetProperty("item").GetProperty("id").GetGuid();

        JsonElement regular = await PostJsonAsync(
            "/api/regular-supplements",
            new
            {
                source = "own",
                supplementId = (Guid?)null,
                ownSupplementId,
                startDate = (string?)null,
                endDate = (string?)null,
                intakeTimes = (string[]?)null
            },
            HttpStatusCode.Created);

        JsonElement item = regular.GetProperty("item");
        item.GetProperty("ownSupplementId").GetGuid().Should().Be(ownSupplementId);
        item.GetProperty("displayName").GetString().Should().Be("Integration protein");
        item.GetProperty("memberSupplements").GetArrayLength().Should().Be(2);

        JsonElement list = await GetJsonAsync("/api/own-supplements");
        list.GetProperty("items").EnumerateArray()
            .Where(own => own.GetProperty("id").GetGuid() == ownSupplementId)
            .Should().ContainSingle()
            .Which.GetProperty("memberCount").GetInt32().Should().Be(2);
    }

    private async Task<List<Guid>> GetSupplementIdsAsync(int count)
    {
        using IServiceScope scope = Factory.Services.CreateScope();
        DrugsAppDbContext dbContext = scope.ServiceProvider.GetRequiredService<DrugsAppDbContext>();

        return await dbContext.Set<Supplement>()
            .AsNoTracking()
            .OrderBy(supplement => supplement.Id)
            .Select(supplement => supplement.Id)
            .Take(count)
            .ToListAsync();
    }

    private async Task SignInAsClientAsync()
    {
        var login = await TestAuthHelper.LoginAsClientAsync(Client, CancellationToken.None);
        TestAuthHelper.ApplyBearerToken(Client, login.AccessToken);
    }

    private async Task<JsonElement> PostJsonAsync(string path, object body, HttpStatusCode expected)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            path,
            body,
            IntegrationTestJson.ApiRequestOptions,
            CancellationToken.None);

        string content = await response.Content.ReadAsStringAsync(CancellationToken.None);
        response.StatusCode.Should().Be(expected, because: content);

        return JsonDocument.Parse(content).RootElement.Clone();
    }

    private async Task<JsonElement> GetJsonAsync(string path)
    {
        HttpResponseMessage response = await Client.GetAsync(path, CancellationToken.None);
        string content = await response.Content.ReadAsStringAsync(CancellationToken.None);
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: content);

        return JsonDocument.Parse(content).RootElement.Clone();
    }
}
