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
}
