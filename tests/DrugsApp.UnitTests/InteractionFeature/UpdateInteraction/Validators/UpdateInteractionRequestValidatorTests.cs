using DrugsApp.Features.InteractionFeature.UpdateInteraction;
using FluentAssertions;

namespace DrugsApp.UnitTests.InteractionFeature.UpdateInteraction.Validators;

public sealed class UpdateInteractionRequestValidatorTests
{
    private readonly UpdateInteractionRequestValidator _validator = new();

    [Fact]
    public async Task Validate_WhenDefaultTextExceedsMaxLength_ReturnsValidationError()
    {
        var request = new UpdateInteractionRequest(
            DefaultText: new string('a', 4001),
            StomachText: null,
            SeverityId: null,
            ClearDefaultText: null,
            ClearStomachText: null);

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateInteractionRequest.DefaultText));
    }

    [Fact]
    public async Task Validate_WhenSeverityIdNull_IsValid()
    {
        var request = new UpdateInteractionRequest(
            DefaultText: "Updated text",
            StomachText: null,
            SeverityId: null,
            ClearDefaultText: null,
            ClearStomachText: null);

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenStomachTextExceedsMaxLength_ReturnsValidationError()
    {
        var request = new UpdateInteractionRequest(
            DefaultText: null,
            StomachText: new string('a', 4001),
            SeverityId: null,
            ClearDefaultText: null,
            ClearStomachText: null);

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateInteractionRequest.StomachText));
    }
}
