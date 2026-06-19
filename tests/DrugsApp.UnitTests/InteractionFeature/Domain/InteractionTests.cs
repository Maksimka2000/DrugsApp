using DrugsApp.Domain.InteractionFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Enums;
using ErrorOr;
using FluentAssertions;

namespace DrugsApp.UnitTests.InteractionFeature.Domain;

public sealed class InteractionTests
{
    [Fact]
    public void Create_WhenBothTextsNull_ReturnsValidationError()
    {
        ErrorOr<Interaction> result = Interaction.Create(
            Guid.NewGuid(),
            defaultText: null,
            stomachText: null);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Description.Should().Be("At least one interaction text is required.");
    }

    [Fact]
    public void InteractionType_SubstanceComesBeforeSubstanceGroup_ForCanonicalOrdering()
    {
        ((int)InteractionType.Substance).Should().BeLessThan((int)InteractionType.SubstanceGroup);
    }

    [Fact]
    public void UpdateTexts_WhenOnlyStomachTextProvided_Succeeds()
    {
        ErrorOr<Interaction> createResult = Interaction.Create(
            Guid.NewGuid(),
            defaultText: "Default",
            stomachText: null);
        Interaction interaction = createResult.Value;

        ErrorOr<Success> updateResult = interaction.UpdateTexts(
            defaultText: null,
            stomachText: "Take with food",
            clearDefaultText: true,
            clearStomachText: false);

        updateResult.IsError.Should().BeFalse();
        interaction.DefaultText.Should().BeNull();
        interaction.StomachText.Should().Be("Take with food");
    }

    [Fact]
    public void UpdateTexts_WhenClearingBothTexts_ReturnsValidationError()
    {
        ErrorOr<Interaction> createResult = Interaction.Create(
            Guid.NewGuid(),
            defaultText: "Default",
            stomachText: "Stomach");
        Interaction interaction = createResult.Value;

        ErrorOr<Success> updateResult = interaction.UpdateTexts(
            defaultText: null,
            stomachText: null,
            clearDefaultText: true,
            clearStomachText: true);

        updateResult.IsError.Should().BeTrue();
        updateResult.FirstError.Description.Should().Be("At least one interaction text is required.");
    }

    [Fact]
    public void UpdateTexts_WhenDefaultTextHasWhitespace_TrimsValue()
    {
        ErrorOr<Interaction> createResult = Interaction.Create(
            Guid.NewGuid(),
            defaultText: "Default",
            stomachText: null);
        Interaction interaction = createResult.Value;

        ErrorOr<Success> updateResult = interaction.UpdateTexts(
            defaultText: "  Trimmed  ",
            stomachText: null,
            clearDefaultText: false,
            clearStomachText: false);

        updateResult.IsError.Should().BeFalse();
        interaction.DefaultText.Should().Be("Trimmed");
    }

    [Fact]
    public void UpdateSeverity_WhenEmptyGuid_DoesNotChangeSeverityId()
    {
        var originalSeverityId = Guid.NewGuid();
        ErrorOr<Interaction> createResult = Interaction.Create(originalSeverityId, "Default", null);
        Interaction interaction = createResult.Value;

        interaction.UpdateSeverity(Guid.Empty);

        interaction.InteractionSeverityId.Should().Be(originalSeverityId);
    }
}
