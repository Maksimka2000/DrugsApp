using System.Net.Http.Json;
using DrugsApp.Domain.InteractionFeature.Enums;
using DrugsApp.Features.ActiveSubstanceGroupFeature.AddActiveSubstanceGroup;
using DrugsApp.Features.ActiveSubstanceGroupFeature.UpdateActiveSubstanceGroup;
using DrugsApp.Features.InteractionFeature.CreateInteraction;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.ActiveSubstanceGroupFeature.UpdateActiveSubstanceGroup;

public sealed class UpdateActiveSubstanceGroupSyncsInteractionExpandedTests : IntegrationTestBase
{
    [Fact]
    public async Task UpdateActiveSubstanceGroup_WhenGroupUsedInInteraction_SyncsInteractionExpandedRows()
    {
        await CatalogTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        var substanceA = await CatalogTestFlows.AddActiveSubstanceAsync(
            Client,
            CatalogTestFlows.CreateUniqueSubstanceName(),
            CancellationToken.None);

        var substanceB = await CatalogTestFlows.AddActiveSubstanceAsync(
            Client,
            CatalogTestFlows.CreateUniqueSubstanceName(),
            CancellationToken.None);

        var substanceC = await CatalogTestFlows.AddActiveSubstanceAsync(
            Client,
            CatalogTestFlows.CreateUniqueSubstanceName(),
            CancellationToken.None);

        var substanceX = await CatalogTestFlows.AddActiveSubstanceAsync(
            Client,
            CatalogTestFlows.CreateUniqueSubstanceName(),
            CancellationToken.None);

        HttpResponseMessage createGroupResponse = await Client.PostAsJsonAsync(
            "/api/active-substance-groups",
            new AddActiveSubstanceGroupRequest(
                CatalogTestFlows.CreateUniqueGroupName(),
                [substanceA.Id, substanceB.Id]),
            IntegrationTestJson.Options,
            CancellationToken.None);

        createGroupResponse.EnsureSuccessStatusCode();

        CreatedActiveSubstanceGroupResponse? createdGroup = await createGroupResponse.Content
            .ReadFromJsonAsync<CreatedActiveSubstanceGroupResponse>(
                IntegrationTestJson.Options,
                CancellationToken.None);

        createdGroup.Should().NotBeNull();

        HttpResponseMessage createInteractionResponse = await Client.PostAsJsonAsync(
            "/api/active-substance-interactions",
            new CreateInteractionRequest(
                Left: new InteractionSideRequest(InteractionType.SubstanceGroup, createdGroup!.Id),
                Right: new InteractionSideRequest(InteractionType.Substance, substanceX.Id),
                SeverityId: InteractionTestSeed.MildSeverityId,
                DefaultText: "Group-substance interaction for expanded sync test.",
                StomachText: null),
            IntegrationTestJson.Options,
            CancellationToken.None);

        createInteractionResponse.EnsureSuccessStatusCode();

        CreatedInteractionResponse? createdInteraction = await createInteractionResponse.Content
            .ReadFromJsonAsync<CreatedInteractionResponse>(
                IntegrationTestJson.Options,
                CancellationToken.None);

        createdInteraction.Should().NotBeNull();

        List<Domain.InteractionFeature.Entities.InteractionExpanded> initialRows =
            await InteractionPersistenceAssertions.GetExpandedRowsAsync(
                Factory.Services,
                createdInteraction!.InteractionId,
                CancellationToken.None);

        initialRows.Should().HaveCount(2);
        InteractionPersistenceAssertions.AssertCanonicalSubstancePairExists(initialRows, substanceA.Id, substanceX.Id);
        InteractionPersistenceAssertions.AssertCanonicalSubstancePairExists(initialRows, substanceB.Id, substanceX.Id);

        HttpResponseMessage updateGroupResponse = await Client.PatchAsJsonAsync(
            $"/api/active-substance-groups/{createdGroup.Id}",
            new UpdateActiveSubstanceGroupRequest(
                Name: null,
                ActiveSubstanceIds: [substanceB.Id, substanceC.Id]),
            IntegrationTestJson.Options,
            CancellationToken.None);

        updateGroupResponse.EnsureSuccessStatusCode();

        List<Domain.InteractionFeature.Entities.InteractionExpanded> updatedRows =
            await InteractionPersistenceAssertions.GetExpandedRowsAsync(
                Factory.Services,
                createdInteraction.InteractionId,
                CancellationToken.None);

        updatedRows.Should().HaveCount(2);
        InteractionPersistenceAssertions.AssertCanonicalSubstancePairMissing(updatedRows, substanceA.Id, substanceX.Id);
        InteractionPersistenceAssertions.AssertCanonicalSubstancePairExists(updatedRows, substanceB.Id, substanceX.Id);
        InteractionPersistenceAssertions.AssertCanonicalSubstancePairExists(updatedRows, substanceC.Id, substanceX.Id);
    }
}
