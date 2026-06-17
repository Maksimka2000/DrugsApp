using DrugsApp.Domain.InteractionFeature.Enums;
using DrugsApp.Features.InteractionFeature.CreateInteraction;
using FluentAssertions;
using FluentValidation.Results;

namespace DrugsApp.UnitTests.InteractionFeature.CreateInteraction.Validators;

public sealed class CreateInteractionRequestValidatorTests
{
    private readonly CreateInteractionRequestValidator _validator = new();

    [Fact]
    public async Task Validate_WhenSelfInteraction_ReturnsValidationError()
    {
        var partyId = Guid.NewGuid();
        var request = new CreateInteractionRequest(
            Left: new InteractionSideRequest(InteractionType.Substance, partyId),
            Right: new InteractionSideRequest(InteractionType.Substance, partyId),
            SeverityId: Guid.NewGuid(),
            DefaultText: "Interaction text",
            StomachText: null);

        ValidationResult? result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.ErrorMessage == "Left and right parties must differ.");
    }

    [Fact]
    public async Task Validate_WhenBothTextsNull_ReturnsValidationError()
    {
        var request = new CreateInteractionRequest(
            Left: new InteractionSideRequest(InteractionType.Substance, Guid.NewGuid()),
            Right: new InteractionSideRequest(InteractionType.Substance, Guid.NewGuid()),
            SeverityId: Guid.NewGuid(),
            DefaultText: null,
            StomachText: null);

        ValidationResult? result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.ErrorMessage == "At least one of default_text or stomach_text must be provided.");
    }
}
