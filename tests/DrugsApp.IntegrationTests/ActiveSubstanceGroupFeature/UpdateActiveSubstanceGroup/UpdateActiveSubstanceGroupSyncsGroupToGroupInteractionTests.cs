using System.Net.Http.Json;
using DrugsApp.Domain.InteractionFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Enums;
using DrugsApp.Features.ActiveSubstanceGroupFeature.AddActiveSubstanceGroup;
using DrugsApp.Features.ActiveSubstanceGroupFeature.UpdateActiveSubstanceGroup;
using DrugsApp.Features.InteractionFeature.CreateInteraction;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.ActiveSubstanceGroupFeature.UpdateActiveSubstanceGroup;

public sealed class UpdateActiveSubstanceGroupSyncsGroupToGroupInteractionTests : IntegrationTestBase
{
    [Fact]
    public async Task UpdateGroup_WhenAddedMembersBelongToTheOtherGroup_ReplacesStalePairsWithoutDuplicates()
    {
        await CatalogTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);
        Guid substanceA = await CreateSubstanceAsync();
        Guid substanceB = await CreateSubstanceAsync();
        Guid substanceC = await CreateSubstanceAsync();
        Guid substanceD = await CreateSubstanceAsync();

        Guid groupG = await CreateGroupAsync(substanceA, substanceD);
        Guid groupH = await CreateGroupAsync(substanceB, substanceC);
        Guid interactionId = await CreateGroupToGroupInteractionAsync(groupG, groupH);

        List<InteractionExpanded> before = await GetRowsAsync(interactionId);
        before.Should().HaveCount(4);

        await UpdateGroupAsync(groupG, substanceB, substanceC, substanceD);

        List<InteractionExpanded> after = await GetRowsAsync(interactionId);
        after.Should().HaveCount(3);
        InteractionPersistenceAssertions.AssertCanonicalSubstancePairExists(after, substanceB, substanceC);
        InteractionPersistenceAssertions.AssertCanonicalSubstancePairExists(after, substanceB, substanceD);
        InteractionPersistenceAssertions.AssertCanonicalSubstancePairExists(after, substanceC, substanceD);
        InteractionPersistenceAssertions.AssertCanonicalSubstancePairMissing(after, substanceA, substanceB);
        InteractionPersistenceAssertions.AssertCanonicalSubstancePairMissing(after, substanceA, substanceC);
    }

    [Fact]
    public async Task UpdateGroup_WhenRemovedPairIsStillJustifiedTheOtherWayRound_KeepsIt()
    {
        await CatalogTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);
        Guid substanceA = await CreateSubstanceAsync();
        Guid substanceB = await CreateSubstanceAsync();
        Guid substanceC = await CreateSubstanceAsync();

        Guid groupG = await CreateGroupAsync(substanceA, substanceB, substanceC);
        Guid groupH = await CreateGroupAsync(substanceB, substanceC);
        Guid interactionId = await CreateGroupToGroupInteractionAsync(groupG, groupH);

        List<InteractionExpanded> before = await GetRowsAsync(interactionId);
        before.Should().HaveCount(3);

        await UpdateGroupAsync(groupG, substanceA, substanceC);

        List<InteractionExpanded> after = await GetRowsAsync(interactionId);
        after.Should().HaveCount(3);
        InteractionPersistenceAssertions.AssertCanonicalSubstancePairExists(after, substanceA, substanceB);
        InteractionPersistenceAssertions.AssertCanonicalSubstancePairExists(after, substanceA, substanceC);
        InteractionPersistenceAssertions.AssertCanonicalSubstancePairExists(after, substanceB, substanceC);
    }

    private async Task<Guid> CreateSubstanceAsync()
    {
        var created = await CatalogTestFlows.AddActiveSubstanceAsync(
            Client,
            CatalogTestFlows.CreateUniqueSubstanceName(),
            CancellationToken.None);

        return created.Id;
    }

    private async Task<Guid> CreateGroupAsync(params Guid[] memberIds)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/active-substance-groups",
            new AddActiveSubstanceGroupRequest(CatalogTestFlows.CreateUniqueGroupName(), memberIds),
            IntegrationTestJson.Options,
            CancellationToken.None);
        response.EnsureSuccessStatusCode();

        CreatedActiveSubstanceGroupResponse? created = await response.Content
            .ReadFromJsonAsync<CreatedActiveSubstanceGroupResponse>(IntegrationTestJson.Options, CancellationToken.None);

        return created!.Id;
    }

    private async Task<Guid> CreateGroupToGroupInteractionAsync(Guid leftGroupId, Guid rightGroupId)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/active-substance-interactions",
            new CreateInteractionRequest(
                Left: new InteractionSideRequest(InteractionType.SubstanceGroup, leftGroupId),
                Right: new InteractionSideRequest(InteractionType.SubstanceGroup, rightGroupId),
                SeverityId: InteractionTestSeed.MildSeverityId,
                DefaultText: "Group-to-group interaction for expanded sync test.",
                StomachText: null),
            IntegrationTestJson.Options,
            CancellationToken.None);
        response.EnsureSuccessStatusCode();

        CreatedInteractionResponse? created = await response.Content
            .ReadFromJsonAsync<CreatedInteractionResponse>(IntegrationTestJson.Options, CancellationToken.None);

        return created!.InteractionId;
    }

    private async Task UpdateGroupAsync(Guid groupId, params Guid[] memberIds)
    {
        HttpResponseMessage response = await Client.PatchAsJsonAsync(
            $"/api/active-substance-groups/{groupId}",
            new UpdateActiveSubstanceGroupRequest(Name: null, ActiveSubstanceIds: memberIds),
            IntegrationTestJson.Options,
            CancellationToken.None);

        string body = await response.Content.ReadAsStringAsync(CancellationToken.None);
        response.IsSuccessStatusCode.Should().BeTrue(because: body);
    }

    private Task<List<InteractionExpanded>> GetRowsAsync(Guid interactionId) =>
        InteractionPersistenceAssertions.GetExpandedRowsAsync(Factory.Services, interactionId, CancellationToken.None);
}
