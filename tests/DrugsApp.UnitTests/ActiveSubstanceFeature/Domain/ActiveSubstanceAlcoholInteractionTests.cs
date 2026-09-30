using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using FluentAssertions;

namespace DrugsApp.UnitTests.ActiveSubstanceFeature.Domain;

public sealed class ActiveSubstanceAlcoholInteractionTests
{
    [Fact]
    public void Create_DefaultsHasAlcoholInteractionToTrue()
    {
        ActiveSubstance substance = ActiveSubstance.Create("Amoxicillin");

        substance.HasAlcoholInteraction.Should().BeTrue();
        substance.AlcoholInteractionOverride.Should().BeNull();
    }

    [Fact]
    public void SetHasAlcoholInteraction_WhenFalse_TurnsFlagOff()
    {
        ActiveSubstance substance = ActiveSubstance.Create("Amoxicillin");

        substance.SetHasAlcoholInteraction(false);

        substance.HasAlcoholInteraction.Should().BeFalse();
    }

    [Fact]
    public void UpsertAlcoholInteractionOverride_WhenFlagWasFalse_ForcesFlagToTrue()
    {
        ActiveSubstance substance = ActiveSubstance.Create("Amoxicillin");
        substance.SetHasAlcoholInteraction(false);

        substance.UpsertAlcoholInteractionOverride("Avoid alcohol.");

        substance.HasAlcoholInteraction.Should().BeTrue();
        substance.AlcoholInteractionOverride!.AlcoholSpecialText.Should().Be("Avoid alcohol.");
    }
}
