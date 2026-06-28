using System.Net;
using DrugsApp.Features.InteractionFeature.CreateInteraction;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.InteractionFeature.DeleteInteraction;

public sealed class DeleteInteractionReturns204Tests : IntegrationTestBase
{
    [Fact]
    public async Task DeleteInteraction_AfterCreate_Returns204AndRemovesPartiesAndExpandedRows()
    {
        await CatalogTestFlows.AuthenticateAsAdminAsync(Client, CancellationToken.None);

        var substance = await CatalogTestFlows.AddActiveSubstanceAsync(
            Client,
            CatalogTestFlows.CreateUniqueSubstanceName(),
            CancellationToken.None);

        var food = await FoodTestFlows.AddFoodAsync(
            Client,
            FoodTestFlows.CreateUniqueFoodName(),
            CancellationToken.None);

        CreatedInteractionResponse created = await InteractionTestFlows.CreateSubstanceFoodInteractionAsync(
            Client,
            substance.Id,
            food.Food.Id,
            CancellationToken.None);

        await InteractionPersistenceAssertions.AssertSubstanceFoodPartiesAndExpandedAsync(
            Factory.Services,
            created.InteractionId,
            substance.Id,
            food.Food.Id,
            CancellationToken.None);

        HttpResponseMessage deleteResponse = await Client.DeleteAsync(
            $"/api/active-substance-interactions/{created.InteractionId}",
            CancellationToken.None);

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        await InteractionPersistenceAssertions.AssertInteractionFullyRemovedAsync(
            Factory.Services,
            created.InteractionId,
            CancellationToken.None);
    }
}
