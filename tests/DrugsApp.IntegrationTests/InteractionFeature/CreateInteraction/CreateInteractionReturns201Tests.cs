using DrugsApp.Domain.InteractionFeature.Enums;
using DrugsApp.Features.InteractionFeature.CreateInteraction;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.InteractionFeature.CreateInteraction;

public sealed class CreateInteractionReturns201Tests : IntegrationTestBase
{
    [Fact]
    public async Task CreateInteraction_SubstanceAndFood_Returns201AndPersistsPartiesAndExpandedRows()
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

        const string defaultText = "Avoid combining during integration tests.";
        const string stomachText = "Take on empty stomach when possible.";

        CreatedInteractionResponse created = await InteractionTestFlows.CreateSubstanceFoodInteractionAsync(
            Client,
            substance.Id,
            food.Food.Id,
            CancellationToken.None,
            defaultText,
            stomachText);

        created.InteractionId.Should().NotBe(Guid.Empty);
        created.Left.Type.Should().Be(InteractionType.Substance);
        created.Left.Id.Should().Be(substance.Id);
        created.Right.Type.Should().Be(InteractionType.Food);
        created.Right.Id.Should().Be(food.Food.Id);
        created.Severity.Id.Should().Be(InteractionTestSeed.MildSeverityId);
        created.DefaultText.Should().Be(defaultText);
        created.StomachText.Should().Be(stomachText);

        await InteractionPersistenceAssertions.AssertSubstanceFoodPartiesAndExpandedAsync(
            Factory.Services,
            created.InteractionId,
            substance.Id,
            food.Food.Id,
            CancellationToken.None);
    }
}
