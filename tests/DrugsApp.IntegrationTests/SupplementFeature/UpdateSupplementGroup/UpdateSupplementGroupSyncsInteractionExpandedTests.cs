using System.Net.Http.Json;
using DrugsApp.Domain.InteractionFeature.Enums;
using DrugsApp.Features.InteractionFeature.CreateInteraction;
using DrugsApp.Features.SupplementFeature.AddSupplement;
using DrugsApp.Features.SupplementFeature.UpdateSupplementGroup;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.SupplementFeature.UpdateSupplementGroup;

public sealed class UpdateSupplementGroupSyncsInteractionExpandedTests : IntegrationTestBase
{
    [Fact]
    public async Task UpdateSupplementGroup_WhenGroupUsedInInteraction_SyncsInteractionExpandedRows()
    {
        await CatalogTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        CreatedSupplementResponse supplementA = await SupplementTestFlows.AddSupplementAsync(
            Client,
            SupplementTestFlows.CreateUniqueSupplementName(),
            CancellationToken.None);

        CreatedSupplementResponse supplementB = await SupplementTestFlows.AddSupplementAsync(
            Client,
            SupplementTestFlows.CreateUniqueSupplementName(),
            CancellationToken.None);

        CreatedSupplementResponse supplementC = await SupplementTestFlows.AddSupplementAsync(
            Client,
            SupplementTestFlows.CreateUniqueSupplementName(),
            CancellationToken.None);

        var substanceX = await CatalogTestFlows.AddActiveSubstanceAsync(
            Client,
            CatalogTestFlows.CreateUniqueSubstanceName(),
            CancellationToken.None);

        var createdGroup = await SupplementTestFlows.AddSupplementGroupAsync(
            Client,
            SupplementTestFlows.CreateUniqueSupplementGroupName(),
            [supplementA.Supplement.Id, supplementB.Supplement.Id],
            CancellationToken.None);

        HttpResponseMessage createInteractionResponse = await Client.PostAsJsonAsync(
            "/api/active-substance-interactions",
            new CreateInteractionRequest(
                Left: new InteractionSideRequest(InteractionType.SupplementGroup, createdGroup.SupplementGroup.Id),
                Right: new InteractionSideRequest(InteractionType.Substance, substanceX.Id),
                SeverityId: InteractionTestSeed.MildSeverityId,
                DefaultText: "Supplement group-substance interaction for expanded sync test.",
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
        InteractionPersistenceAssertions.AssertCanonicalSubstanceSupplementPairExists(
            initialRows,
            substanceX.Id,
            supplementA.Supplement.Id);
        InteractionPersistenceAssertions.AssertCanonicalSubstanceSupplementPairExists(
            initialRows,
            substanceX.Id,
            supplementB.Supplement.Id);

        HttpResponseMessage updateGroupResponse = await Client.PatchAsJsonAsync(
            $"/api/supplement-groups/{createdGroup.SupplementGroup.Id}",
            new UpdateSupplementGroupRequest(
                Name: null,
                SupplementIds: [supplementB.Supplement.Id, supplementC.Supplement.Id]),
            IntegrationTestJson.Options,
            CancellationToken.None);

        updateGroupResponse.EnsureSuccessStatusCode();

        List<Domain.InteractionFeature.Entities.InteractionExpanded> updatedRows =
            await InteractionPersistenceAssertions.GetExpandedRowsAsync(
                Factory.Services,
                createdInteraction.InteractionId,
                CancellationToken.None);

        updatedRows.Should().HaveCount(2);
        InteractionPersistenceAssertions.AssertCanonicalSubstanceSupplementPairMissing(
            updatedRows,
            substanceX.Id,
            supplementA.Supplement.Id);
        InteractionPersistenceAssertions.AssertCanonicalSubstanceSupplementPairExists(
            updatedRows,
            substanceX.Id,
            supplementB.Supplement.Id);
        InteractionPersistenceAssertions.AssertCanonicalSubstanceSupplementPairExists(
            updatedRows,
            substanceX.Id,
            supplementC.Supplement.Id);
    }
}
