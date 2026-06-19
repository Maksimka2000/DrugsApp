using DrugsApp.Domain.InteractionFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Enums;
using ErrorOr;
using FluentAssertions;

namespace DrugsApp.UnitTests.InteractionFeature.Domain;

public sealed class InteractionPartyTests
{
    [Fact]
    public void Create_WhenPartyIdEmpty_ReturnsValidationError()
    {
        ErrorOr<InteractionParty> result = InteractionParty.Create(
            Guid.NewGuid(),
            InteractionType.Substance,
            Guid.Empty);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Description.Should().Be("PartyId must not be empty.");
    }

    [Fact]
    public void Create_WhenSubstance_SetsActiveSubstanceId()
    {
        var interactionId = Guid.NewGuid();
        var substanceId = Guid.NewGuid();

        ErrorOr<InteractionParty> result = InteractionParty.Create(
            interactionId,
            InteractionType.Substance,
            substanceId);

        result.IsError.Should().BeFalse();
        result.Value.PartyType.Should().Be(InteractionType.Substance);
        result.Value.ActiveSubstanceId.Should().Be(substanceId);
    }

    [Fact]
    public void Create_WhenSubstanceGroup_SetsActiveSubstanceGroupId()
    {
        var groupId = Guid.NewGuid();

        ErrorOr<InteractionParty> result = InteractionParty.Create(
            Guid.NewGuid(),
            InteractionType.SubstanceGroup,
            groupId);

        result.IsError.Should().BeFalse();
        result.Value.ActiveSubstanceGroupId.Should().Be(groupId);
    }
}
